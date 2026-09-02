#pragma warning disable CA1707

using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Tests;

public sealed class CustomerRegistrationServiceTests
{
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
        var registrationService = new CustomerRegistrationService(dbContext, passwordService);

        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("alice@example.com", response.Email);
        Assert.Equal("Alice", response.FirstName);
        Assert.Equal("Smith", response.LastName);
        Assert.True(response.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailByTrimmingAndConvertingToUpperInvariant()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = new CustomerRegistrationService(dbContext, passwordService);

        var request = new RegisterRequest(
            Email: "  alice@example.com  ",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert - verify persisted customer has normalized email
        var persistedCustomer = await dbContext.Customers.SingleAsync(c => c.Id == response.Id);
        Assert.Equal("ALICE@EXAMPLE.COM", persistedCustomer.NormalizedEmail);
        Assert.Equal("  alice@example.com  ", persistedCustomer.Email); // Original email preserved
    }

    [Fact]
    public async Task RegisterAsync_HashesPasswordAndNeverStoresPlaintext()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = new CustomerRegistrationService(dbContext, passwordService);

        const string plainTextPassword = "SecurePassword123!";
        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: plainTextPassword,
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert
        var persistedCustomer = await dbContext.Customers.SingleAsync(c => c.Id == response.Id);

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
        var registrationService = new CustomerRegistrationService(dbContext, passwordService);

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
        var registrationService = new CustomerRegistrationService(dbContext, passwordService);

        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert
        var persistedCustomer = await dbContext.Customers.SingleAsync(c => c.Id == response.Id);
        Assert.True(persistedCustomer.IsActive);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsCustomerResponseWithoutPasswordHash()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var passwordService = new PasswordService();
        var registrationService = new CustomerRegistrationService(dbContext, passwordService);

        var request = new RegisterRequest(
            Email: "alice@example.com",
            Password: "SecurePassword123!",
            FirstName: "Alice",
            LastName: "Smith");

        // Act
        var response = await registrationService.RegisterAsync(request, CancellationToken.None);

        // Assert - CustomerResponse record does not contain PasswordHash or NormalizedEmail properties
        Assert.IsType<CustomerResponse>(response);
        Assert.Equal("alice@example.com", response.Email);
        Assert.Equal("Alice", response.FirstName);
        Assert.Equal("Smith", response.LastName);

        // Verify via reflection that response type doesn't have these sensitive fields
        var responseType = response.GetType();
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
        var registrationService = new CustomerRegistrationService(dbContext, passwordService);

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
        var persistedCustomer = await dbContext.Customers.SingleAsync(c => c.Id == response.Id);

        // Both should be within the time range and equal
        Assert.True(persistedCustomer.CreatedAt >= beforeRegistration);
        Assert.True(persistedCustomer.CreatedAt <= afterRegistration);
        Assert.Equal(persistedCustomer.CreatedAt, persistedCustomer.UpdatedAt);
    }
}

#pragma warning restore CA1707
