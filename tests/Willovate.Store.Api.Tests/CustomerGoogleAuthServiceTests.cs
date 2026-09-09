#pragma warning disable CA1707

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Willovate.Store.Api.Configuration;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Tests;

public sealed class TestGoogleTokenValidator : IGoogleTokenValidator
{
    private readonly Dictionary<string, GoogleUserPayload> validTokens = new();

    public void AddValidToken(string idToken, GoogleUserPayload payload)
    {
        validTokens[idToken] = payload;
    }

    public Task<GoogleUserPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (validTokens.TryGetValue(idToken, out var payload))
        {
            return Task.FromResult<GoogleUserPayload?>(payload);
        }

        return Task.FromResult<GoogleUserPayload?>(null);
    }
}

public sealed class CustomerGoogleAuthServiceTests
{
    private static StoreDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StoreDbContext(options);
    }

    private static (CustomerGoogleAuthService service, TestGoogleTokenValidator validator) CreateService(StoreDbContext dbContext)
    {
        var validator = new TestGoogleTokenValidator();
        var jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "test-secret-key-must-be-at-least-32-characters-long-for-hs256",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 60
        });

        var jwtTokenService = new JwtTokenService(jwtOptions);
        var service = new CustomerGoogleAuthService(dbContext, validator, jwtTokenService);

        return (service, validator);
    }

    [Fact]
    public async Task GoogleTokenValidator_ThrowsInvalidOperationException_WhenClientIdIsMissing()
    {
        var options = Options.Create(new GoogleOptions { ClientId = "   " });
        var validator = new GoogleTokenValidator(options);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            validator.ValidateAsync("any-token", CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticateGoogleUserAsync_CreatesNewCustomerWithSentinelPasswordHash()
    {
        using var dbContext = CreateInMemoryDbContext();
        var (service, validator) = CreateService(dbContext);

        validator.AddValidToken("valid-token", new GoogleUserPayload(
            Subject: "google-123",
            Email: "newuser@example.com",
            EmailVerified: true,
            GivenName: "New",
            FamilyName: "User",
            Name: "New User"));

        var response = await service.AuthenticateGoogleUserAsync(new GoogleAuthRequest("valid-token"), CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotEmpty(response.AccessToken);
        Assert.Equal("newuser@example.com", response.Customer.Email);
        Assert.Equal("New", response.Customer.FirstName);
        Assert.Equal("User", response.Customer.LastName);

        var dbCustomer = await dbContext.Customers.SingleOrDefaultAsync(c => c.NormalizedEmail == "NEWUSER@EXAMPLE.COM");
        Assert.NotNull(dbCustomer);
        Assert.True(dbCustomer.IsActive);
        Assert.Equal("EXTERNAL_AUTH_NO_PASSWORD", dbCustomer.PasswordHash);

        var passwordService = new PasswordService();
        Assert.False(passwordService.VerifyPassword("AnyPassword123!", dbCustomer.PasswordHash));
    }

    [Fact]
    public async Task AuthenticateGoogleUserAsync_AuthenticatesExistingCustomerWithoutCreatingDuplicate()
    {
        using var dbContext = CreateInMemoryDbContext();
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            NormalizedEmail = "EXISTING@EXAMPLE.COM",
            PasswordHash = "hashedpassword",
            FirstName = "Existing",
            LastName = "Customer",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            IsActive = true
        };
        dbContext.Customers.Add(existingCustomer);
        await dbContext.SaveChangesAsync();

        var (service, validator) = CreateService(dbContext);
        validator.AddValidToken("valid-token", new GoogleUserPayload(
            Subject: "google-456",
            Email: "existing@example.com",
            EmailVerified: true,
            GivenName: "Existing",
            FamilyName: "Customer",
            Name: "Existing Customer"));

        var response = await service.AuthenticateGoogleUserAsync(new GoogleAuthRequest("valid-token"), CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotEmpty(response.AccessToken);
        Assert.Equal(existingCustomer.Id, response.Customer.Id);

        var count = await dbContext.Customers.CountAsync(c => c.NormalizedEmail == "EXISTING@EXAMPLE.COM");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AuthenticateGoogleUserAsync_NormalizesEmailForCaseInsensitiveLookup()
    {
        using var dbContext = CreateInMemoryDbContext();
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "CaseSensitive@Example.COM",
            NormalizedEmail = "CASESENSITIVE@EXAMPLE.COM",
            PasswordHash = "hashedpassword",
            FirstName = "Case",
            LastName = "User",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
        dbContext.Customers.Add(existingCustomer);
        await dbContext.SaveChangesAsync();

        var (service, validator) = CreateService(dbContext);
        validator.AddValidToken("valid-token", new GoogleUserPayload(
            Subject: "google-789",
            Email: "  casesensitive@example.com  ",
            EmailVerified: true,
            GivenName: "Case",
            FamilyName: "User",
            Name: "Case User"));

        var response = await service.AuthenticateGoogleUserAsync(new GoogleAuthRequest("valid-token"), CancellationToken.None);

        Assert.Equal(existingCustomer.Id, response.Customer.Id);
    }

    [Fact]
    public async Task AuthenticateGoogleUserAsync_RejectsInvalidGoogleToken()
    {
        using var dbContext = CreateInMemoryDbContext();
        var (service, _) = CreateService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AuthenticateGoogleUserAsync(new GoogleAuthRequest("invalid-token"), CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticateGoogleUserAsync_RejectsUnverifiedEmailToken()
    {
        using var dbContext = CreateInMemoryDbContext();
        var (service, validator) = CreateService(dbContext);

        validator.AddValidToken("unverified-token", new GoogleUserPayload(
            Subject: "google-999",
            Email: "unverified@example.com",
            EmailVerified: false,
            GivenName: "Unverified",
            FamilyName: "User",
            Name: "Unverified User"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AuthenticateGoogleUserAsync(new GoogleAuthRequest("unverified-token"), CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticateGoogleUserAsync_RejectsInactiveCustomer()
    {
        using var dbContext = CreateInMemoryDbContext();
        var inactiveCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            NormalizedEmail = "INACTIVE@EXAMPLE.COM",
            PasswordHash = "hashedpassword",
            FirstName = "Inactive",
            LastName = "Customer",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = false
        };
        dbContext.Customers.Add(inactiveCustomer);
        await dbContext.SaveChangesAsync();

        var (service, validator) = CreateService(dbContext);
        validator.AddValidToken("valid-token", new GoogleUserPayload(
            Subject: "google-000",
            Email: "inactive@example.com",
            EmailVerified: true,
            GivenName: "Inactive",
            FamilyName: "Customer",
            Name: "Inactive Customer"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AuthenticateGoogleUserAsync(new GoogleAuthRequest("valid-token"), CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticateGoogleUserAsync_ReturnsCustomerResponseWithoutSensitiveFields()
    {
        using var dbContext = CreateInMemoryDbContext();
        var (service, validator) = CreateService(dbContext);

        validator.AddValidToken("valid-token", new GoogleUserPayload(
            Subject: "google-111",
            Email: "secure@example.com",
            EmailVerified: true,
            GivenName: "Secure",
            FamilyName: "User",
            Name: "Secure User"));

        var response = await service.AuthenticateGoogleUserAsync(new GoogleAuthRequest("valid-token"), CancellationToken.None);

        Assert.IsType<AuthResponse>(response);
        Assert.NotEmpty(response.AccessToken);
        Assert.Equal("secure@example.com", response.Customer.Email);

        var responseType = response.Customer.GetType();
        Assert.Null(responseType.GetProperty("PasswordHash"));
        Assert.Null(responseType.GetProperty("NormalizedEmail"));
        Assert.Null(responseType.GetProperty("IsActive"));
    }
}

#pragma warning restore CA1707
