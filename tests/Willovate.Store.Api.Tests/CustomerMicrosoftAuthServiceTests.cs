#pragma warning disable CA1707

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Willovate.Store.Api.Configuration;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Tests;

public sealed class TestMicrosoftTokenValidator : IMicrosoftTokenValidator
{
    private readonly Dictionary<string, MicrosoftUserPayload> validTokens = new();

    public void AddValidToken(string idToken, MicrosoftUserPayload payload)
    {
        validTokens[idToken] = payload;
    }

    public Task<MicrosoftUserPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (validTokens.TryGetValue(idToken, out var payload))
        {
            return Task.FromResult<MicrosoftUserPayload?>(payload);
        }

        return Task.FromResult<MicrosoftUserPayload?>(null);
    }
}

public sealed class CustomerMicrosoftAuthServiceTests
{
    private static StoreDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StoreDbContext(options);
    }

    private static (CustomerMicrosoftAuthService service, TestMicrosoftTokenValidator validator) CreateService(StoreDbContext dbContext)
    {
        var validator = new TestMicrosoftTokenValidator();
        var jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "test-secret-key-must-be-at-least-32-characters-long-for-hs256",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 60
        });

        var jwtTokenService = new JwtTokenService(jwtOptions);
        var service = new CustomerMicrosoftAuthService(dbContext, validator, jwtTokenService);

        return (service, validator);
    }

    private static (MicrosoftTokenValidator validator, string signingKey) CreateTestTokenValidator(string clientId, string tenantId = "common")
    {
        var signingKey = "test-signing-key-32-chars-minimum-length!!";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var openIdConfig = new OpenIdConnectConfiguration();
        openIdConfig.SigningKeys.Add(securityKey);

        var staticConfigManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(openIdConfig);
        var options = Options.Create(new MicrosoftOptions { ClientId = clientId, TenantId = tenantId });

        return (new MicrosoftTokenValidator(options, staticConfigManager), signingKey);
    }

    private static string CreateSignedJwt(string issuer, string audience, string signingKey, DateTime? expires = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", "ms-test-sub-123"),
                new Claim("email", "testjwt@example.com"),
                new Claim("given_name", "Test"),
                new Claim("family_name", "User")
            ]),
            Issuer = issuer,
            Audience = audience,
            Expires = expires ?? DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Fact]
    public async Task MicrosoftTokenValidator_ThrowsInvalidOperationException_WhenClientIdIsMissing()
    {
        var options = Options.Create(new MicrosoftOptions { ClientId = "   " });
        var validator = new MicrosoftTokenValidator(options);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            validator.ValidateAsync("any-token", CancellationToken.None));
    }

    [Fact]
    public async Task MicrosoftTokenValidator_AcceptsCorrectIssuer_ForMultiTenantCommon()
    {
        var (validator, signingKey) = CreateTestTokenValidator("my-client-id", "common");
        var validIssuer = "https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0";
        var token = CreateSignedJwt(validIssuer, "my-client-id", signingKey);

        var payload = await validator.ValidateAsync(token);

        Assert.NotNull(payload);
        Assert.Equal("ms-test-sub-123", payload.Subject);
        Assert.Equal("testjwt@example.com", payload.Email);
    }

    [Fact]
    public async Task MicrosoftTokenValidator_RejectsWrongIssuer_ForMultiTenantCommon()
    {
        var (validator, signingKey) = CreateTestTokenValidator("my-client-id", "common");
        var wrongIssuer = "https://untrusted-issuer.com/v2.0";
        var token = CreateSignedJwt(wrongIssuer, "my-client-id", signingKey);

        var payload = await validator.ValidateAsync(token);

        Assert.Null(payload);
    }

    [Fact]
    public async Task MicrosoftTokenValidator_AcceptsCorrectIssuer_ForSingleTenant()
    {
        var tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";
        var (validator, signingKey) = CreateTestTokenValidator("my-client-id", tenantId);
        var validIssuer = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        var token = CreateSignedJwt(validIssuer, "my-client-id", signingKey);

        var payload = await validator.ValidateAsync(token);

        Assert.NotNull(payload);
        Assert.Equal("ms-test-sub-123", payload.Subject);
    }

    [Fact]
    public async Task MicrosoftTokenValidator_RejectsWrongIssuer_ForSingleTenant()
    {
        var configuredTenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";
        var wrongTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";
        var (validator, signingKey) = CreateTestTokenValidator("my-client-id", configuredTenantId);
        var wrongIssuer = $"https://login.microsoftonline.com/{wrongTenantId}/v2.0";
        var token = CreateSignedJwt(wrongIssuer, "my-client-id", signingKey);

        var payload = await validator.ValidateAsync(token);

        Assert.Null(payload);
    }

    [Fact]
    public async Task MicrosoftTokenValidator_AcceptsCorrectAudience()
    {
        var (validator, signingKey) = CreateTestTokenValidator("correct-client-id", "common");
        var validIssuer = "https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0";
        var token = CreateSignedJwt(validIssuer, "correct-client-id", signingKey);

        var payload = await validator.ValidateAsync(token);

        Assert.NotNull(payload);
    }

    [Fact]
    public async Task MicrosoftTokenValidator_RejectsWrongAudience()
    {
        var (validator, signingKey) = CreateTestTokenValidator("correct-client-id", "common");
        var validIssuer = "https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0";
        var token = CreateSignedJwt(validIssuer, "wrong-client-id", signingKey);

        var payload = await validator.ValidateAsync(token);

        Assert.Null(payload);
    }

    [Fact]
    public async Task MicrosoftTokenValidator_RejectsExpiredToken()
    {
        var (validator, signingKey) = CreateTestTokenValidator("my-client-id", "common");
        var validIssuer = "https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0";
        var expiredToken = CreateSignedJwt(validIssuer, "my-client-id", signingKey, expires: DateTime.UtcNow.AddHours(-1));

        var payload = await validator.ValidateAsync(expiredToken);

        Assert.Null(payload);
    }

    [Fact]
    public async Task AuthenticateMicrosoftUserAsync_CreatesNewCustomerWithSentinelPasswordHash()
    {
        using var dbContext = CreateInMemoryDbContext();
        var (service, validator) = CreateService(dbContext);

        validator.AddValidToken("valid-token", new MicrosoftUserPayload(
            Subject: "ms-sub-123",
            Email: "msuser@example.com",
            GivenName: "Microsoft",
            FamilyName: "User",
            Name: "Microsoft User"));

        var response = await service.AuthenticateMicrosoftUserAsync(new MicrosoftAuthRequest("valid-token"), CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotEmpty(response.AccessToken);
        Assert.Equal("msuser@example.com", response.Customer.Email);
        Assert.Equal("Microsoft", response.Customer.FirstName);
        Assert.Equal("User", response.Customer.LastName);

        var dbCustomer = await dbContext.Customers.SingleOrDefaultAsync(c => c.NormalizedEmail == "MSUSER@EXAMPLE.COM");
        Assert.NotNull(dbCustomer);
        Assert.True(dbCustomer.IsActive);
        Assert.Equal("EXTERNAL_AUTH_NO_PASSWORD", dbCustomer.PasswordHash);

        var passwordService = new PasswordService();
        Assert.False(passwordService.VerifyPassword("AnyPassword123!", dbCustomer.PasswordHash));
    }

    [Fact]
    public async Task AuthenticateMicrosoftUserAsync_AuthenticatesExistingCustomerWithoutCreatingDuplicate()
    {
        using var dbContext = CreateInMemoryDbContext();
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "msexisting@example.com",
            NormalizedEmail = "MSEXISTING@EXAMPLE.COM",
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
        validator.AddValidToken("valid-token", new MicrosoftUserPayload(
            Subject: "ms-sub-456",
            Email: "msexisting@example.com",
            GivenName: "Existing",
            FamilyName: "Customer",
            Name: "Existing Customer"));

        var response = await service.AuthenticateMicrosoftUserAsync(new MicrosoftAuthRequest("valid-token"), CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotEmpty(response.AccessToken);
        Assert.Equal(existingCustomer.Id, response.Customer.Id);

        var count = await dbContext.Customers.CountAsync(c => c.NormalizedEmail == "MSEXISTING@EXAMPLE.COM");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AuthenticateMicrosoftUserAsync_NormalizesEmailForCaseInsensitiveLookup()
    {
        using var dbContext = CreateInMemoryDbContext();
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "MsCaseSensitive@Example.COM",
            NormalizedEmail = "MSCASESENSITIVE@EXAMPLE.COM",
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
        validator.AddValidToken("valid-token", new MicrosoftUserPayload(
            Subject: "ms-sub-789",
            Email: "  mscasesensitive@example.com  ",
            GivenName: "Case",
            FamilyName: "User",
            Name: "Case User"));

        var response = await service.AuthenticateMicrosoftUserAsync(new MicrosoftAuthRequest("valid-token"), CancellationToken.None);

        Assert.Equal(existingCustomer.Id, response.Customer.Id);
    }

    [Fact]
    public async Task AuthenticateMicrosoftUserAsync_RejectsInvalidMicrosoftToken()
    {
        using var dbContext = CreateInMemoryDbContext();
        var (service, _) = CreateService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AuthenticateMicrosoftUserAsync(new MicrosoftAuthRequest("invalid-token"), CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticateMicrosoftUserAsync_RejectsInactiveCustomer()
    {
        using var dbContext = CreateInMemoryDbContext();
        var inactiveCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "msinactive@example.com",
            NormalizedEmail = "MSINACTIVE@EXAMPLE.COM",
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
        validator.AddValidToken("valid-token", new MicrosoftUserPayload(
            Subject: "ms-sub-000",
            Email: "msinactive@example.com",
            GivenName: "Inactive",
            FamilyName: "Customer",
            Name: "Inactive Customer"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AuthenticateMicrosoftUserAsync(new MicrosoftAuthRequest("valid-token"), CancellationToken.None));
    }

    [Fact]
    public async Task AuthenticateMicrosoftUserAsync_ReturnsCustomerResponseWithoutSensitiveFields()
    {
        using var dbContext = CreateInMemoryDbContext();
        var (service, validator) = CreateService(dbContext);

        validator.AddValidToken("valid-token", new MicrosoftUserPayload(
            Subject: "ms-sub-111",
            Email: "mssecure@example.com",
            GivenName: "Secure",
            FamilyName: "User",
            Name: "Secure User"));

        var response = await service.AuthenticateMicrosoftUserAsync(new MicrosoftAuthRequest("valid-token"), CancellationToken.None);

        Assert.IsType<AuthResponse>(response);
        Assert.NotEmpty(response.AccessToken);
        Assert.Equal("mssecure@example.com", response.Customer.Email);

        var responseType = response.Customer.GetType();
        Assert.Null(responseType.GetProperty("PasswordHash"));
        Assert.Null(responseType.GetProperty("NormalizedEmail"));
        Assert.Null(responseType.GetProperty("IsActive"));
    }
}

#pragma warning restore CA1707
