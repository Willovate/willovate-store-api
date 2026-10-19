#pragma warning disable CA1707

using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Willovate.Store.Api.Configuration;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Tests;

public sealed class CustomerRegistrationServiceTests
{
    private static CustomerRegistrationService CreateRegistrationService(
        StoreDbContext dbContext,
        IPasswordService passwordService)
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "test-secret-key-must-be-at-least-32-characters-long-for-hs256",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 60
        });

        return new CustomerRegistrationService(
            dbContext,
            passwordService,
            new JwtTokenService(jwtOptions));
    }

    private static StoreDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StoreDbContext(options);
    }

    [Fact]
    public async Task RegisterAsync_SuccessfullyRegistersNewCustomer()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = CreateRegistrationService(dbContext, passwordService);

        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert
        Assert.NotEmpty(response.AccessToken);
        Assert.NotEqual(Guid.Empty, response.Customer.Id);
        Assert.Equal("alice@example.com", response.Customer.Email);
        Assert.Equal("Alice", response.Customer.FirstName);
        Assert.Equal("Smith", response.Customer.LastName);
        Assert.True(response.Customer.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsTokenWithNewCustomerSubject()
    {
        using var dbContext = CreateInMemoryDbContext();
        var registrationService = CreateRegistrationService(dbContext, new PasswordService());
        var request = new RegisterRequest(
            Email: "token@example.com",
            Password: "SecurePassword123!",
            FirstName: "Token",
            LastName: "User");

        var response = await registrationService.RegisterAsync(request, CancellationToken.None);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);

        Assert.Equal(response.Customer.Id.ToString(), token.Subject);
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailByTrimmingAndConvertingToUpperInvariant()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = CreateRegistrationService(dbContext, passwordService);

        var request = new RegisterRequest(
            Email: "  alice@example.com  ",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert - verify persisted customer has normalized email
        var persistedCustomer = await dbContext.Customers.SingleAsync(c => c.Id == response.Customer.Id);
        Assert.Equal("ALICE@EXAMPLE.COM", persistedCustomer.NormalizedEmail);
        Assert.Equal("  alice@example.com  ", persistedCustomer.Email); // Original email preserved
    }

    [Fact]
    public async Task RegisterAsync_HashesPasswordAndNeverStoresPlaintext()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = CreateRegistrationService(dbContext, passwordService);

        const string plainTextPassword = "SecurePassword123!";
        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: plainTextPassword,
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert
        var persistedCustomer = await dbContext.Customers.SingleAsync(c => c.Id == response.Customer.Id);

        // Password hash should not be plaintext
        Assert.NotEqual(plainTextPassword, persistedCustomer.PasswordHash);

        // Hash should be verifiable by PasswordService
        Assert.True(passwordService.VerifyPassword(plainTextPassword, persistedCustomer.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_RejectsNormalizedEmailDuplicate()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = CreateRegistrationService(dbContext, passwordService);

        var firstRequest = new RegisterRequest(
            Email: "alice@example.com",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        var secondRequest = new RegisterRequest(
            Email: "ALICE@EXAMPLE.COM", // Different casing, same normalized form
            Password: "AnotherPassword456!",
            FirstName: "Alice",
            LastName: "Jones");

        // Act & Assert
        await registrationService.RegisterAsync(firstRequest, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => registrationService.RegisterAsync(secondRequest, CancellationToken.None));

        Assert.Contains("already registered", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_PersistsCustomerWithIsActiveTrue()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = CreateRegistrationService(dbContext, passwordService);

        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert
        var persistedCustomer = await dbContext.Customers.SingleAsync(c => c.Id == response.Customer.Id);
        Assert.True(persistedCustomer.IsActive);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsCustomerResponseWithoutPasswordHash()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = CreateRegistrationService(dbContext, passwordService);

        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert - CustomerResponse record does not contain PasswordHash or NormalizedEmail properties
        Assert.IsType<AuthResponse>(response);
        Assert.Equal("alice@example.com", response.Customer.Email);
        Assert.Equal("Alice", response.Customer.FirstName);
        Assert.Equal("Smith", response.Customer.LastName);

        // Verify via reflection that response type doesn't have these sensitive fields
        var responseType = response.Customer.GetType();
        Assert.Null(responseType.GetProperty("PasswordHash"));
        Assert.Null(responseType.GetProperty("NormalizedEmail"));
        Assert.Null(responseType.GetProperty("IsActive"));
    }

    [Fact]
    public async Task RegisterAsync_SetsCreatedAtAndUpdatedAtToSameUtcNowValue()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = CreateRegistrationService(dbContext, passwordService);

        var beforeRegistration = DateTimeOffset.UtcNow;

        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        var afterRegistration = DateTimeOffset.UtcNow;

        // Assert
        var persistedCustomer = await dbContext.Customers.SingleAsync(c => c.Id == response.Customer.Id);

        // Both should be within the time range and equal
        Assert.True(persistedCustomer.CreatedAt >= beforeRegistration);
        Assert.True(persistedCustomer.CreatedAt <= afterRegistration);
        Assert.Equal(persistedCustomer.CreatedAt, persistedCustomer.UpdatedAt);
    }
}

#pragma warning restore CA1707
