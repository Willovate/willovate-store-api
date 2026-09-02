#pragma warning disable CA1707

using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Tests;

public sealed class CustomerLoginServiceTests
{
    private static StoreDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StoreDbContext(options);
    }

    private static async Task<Customer> SeedCustomerAsync(
        StoreDbContext dbContext,
        string email = "login@example.com",
        string password = "CorrectPassword123!",
        bool isActive = true)
    {
        var passwordService = new PasswordService();
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var passwordHash = passwordService.HashPassword(password);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHash,
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = isActive
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        return customer;
    }

    [Fact]
    public async Task LoginAsync_SuccessfullyAuthenticatesValidCredentials()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        await SeedCustomerAsync(dbContext, "alice@example.com", "SecurePassword123!");

        var passwordService = new PasswordService();
        var loginService = new CustomerLoginService(dbContext, passwordService);

        var request = new LoginRequest(Email: "alice@example.com", Password: "SecurePassword123!");

        // Act
        var response = await loginService.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("alice@example.com", response.Email);
        Assert.Equal("Test", response.FirstName);
        Assert.Equal("User", response.LastName);
    }

    [Fact]
    public async Task LoginAsync_NormalizesEmailForCaseInsensitiveLookup()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        await SeedCustomerAsync(dbContext, "CaseSensitive@Example.COM", "CorrectPassword123!");

        var passwordService = new PasswordService();
        var loginService = new CustomerLoginService(dbContext, passwordService);

        // Request with different casing
        var request = new LoginRequest(Email: "  casesensitive@example.com  ", Password: "CorrectPassword123!");

        // Act
        var response = await loginService.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("CaseSensitive@Example.COM", response.Email);
    }

    [Fact]
    public async Task LoginAsync_RejectsUnknownEmail()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        await SeedCustomerAsync(dbContext);

        var passwordService = new PasswordService();
        var loginService = new CustomerLoginService(dbContext, passwordService);

        var request = new LoginRequest(Email: "unknown@example.com", Password: "CorrectPassword123!");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => loginService.LoginAsync(request, CancellationToken.None));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_RejectsIncorrectPassword()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        await SeedCustomerAsync(dbContext, "bob@example.com", "CorrectPassword123!");

        var passwordService = new PasswordService();
        var loginService = new CustomerLoginService(dbContext, passwordService);

        var request = new LoginRequest(Email: "bob@example.com", Password: "IncorrectPassword456!");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => loginService.LoginAsync(request, CancellationToken.None));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_RejectsInactiveCustomer()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        await SeedCustomerAsync(dbContext, "inactive@example.com", "CorrectPassword123!", isActive: false);

        var passwordService = new PasswordService();
        var loginService = new CustomerLoginService(dbContext, passwordService);

        var request = new LoginRequest(Email: "inactive@example.com", Password: "CorrectPassword123!");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => loginService.LoginAsync(request, CancellationToken.None));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentialsDoNotRevealEmailExistence()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        await SeedCustomerAsync(dbContext, "exists@example.com", "CorrectPassword123!");

        var passwordService = new PasswordService();
        var loginService = new CustomerLoginService(dbContext, passwordService);

        var unknownEmailRequest = new LoginRequest(Email: "unknown@example.com", Password: "AnyPassword123!");
        var wrongPasswordRequest = new LoginRequest(Email: "exists@example.com", Password: "WrongPassword123!");

        // Act
        var unknownEmailException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => loginService.LoginAsync(unknownEmailRequest, CancellationToken.None));

        var wrongPasswordException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => loginService.LoginAsync(wrongPasswordRequest, CancellationToken.None));

        // Assert - both throw the SAME generic message
        Assert.Equal("Invalid email or password.", unknownEmailException.Message);
        Assert.Equal("Invalid email or password.", wrongPasswordException.Message);
    }

    [Fact]
    public async Task LoginAsync_UsesPasswordServiceForVerification()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        const string password = "CorrectPassword123!";
        await SeedCustomerAsync(dbContext, "verify@example.com", password);

        var passwordService = new PasswordService();
        var loginService = new CustomerLoginService(dbContext, passwordService);

        var request = new LoginRequest(Email: "verify@example.com", Password: password);

        // Act
        var response = await loginService.LoginAsync(request, CancellationToken.None);

        // Assert - successful login proves password was verified by PasswordService
        Assert.NotNull(response);
        Assert.Equal("verify@example.com", response.Email);
    }

    [Fact]
    public async Task LoginAsync_ReturnsCustomerResponseWithoutSensitiveFields()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        await SeedCustomerAsync(dbContext, "secure@example.com", "CorrectPassword123!");

        var passwordService = new PasswordService();
        var loginService = new CustomerLoginService(dbContext, passwordService);

        var request = new LoginRequest(Email: "secure@example.com", Password: "CorrectPassword123!");

        // Act
        var response = await loginService.LoginAsync(request, CancellationToken.None);

        // Assert - CustomerResponse record does not contain sensitive fields
        Assert.IsType<CustomerResponse>(response);

        var responseType = response.GetType();
        Assert.Null(responseType.GetProperty("PasswordHash"));
        Assert.Null(responseType.GetProperty("NormalizedEmail"));
        Assert.Null(responseType.GetProperty("IsActive"));
    }
}

#pragma warning restore CA1707
