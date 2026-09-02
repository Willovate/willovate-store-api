#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;

namespace Willovate.Store.Api.Tests;

public sealed class StoreApiTests : IAsyncLifetime
{
    private WebApplicationFactory<Program>? factory;
    private HttpClient? client;

    public Task InitializeAsync()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Secret", "test-secret-key-must-be-at-least-32-characters-long-for-hs256");
            builder.UseSetting("Jwt:Issuer", "test-issuer");
            builder.UseSetting("Jwt:Audience", "test-audience");
            builder.UseSetting("Jwt:ExpirationMinutes", "60");
        });

        client = factory.CreateClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task HealthEndpointReportsAHealthyService()
    {
        var response = await client!.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ProductsEndpointReturnsSeededFeaturedProducts()
    {
        var response = await client!.GetAsync("/api/products?featured=true&pageSize=20");
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, product => Assert.True(product.IsFeatured));
    }

    [Fact]
    public async Task UnknownProductReturnsProblemDetails()
    {
        var response = await client!.GetAsync("/api/products/not-a-real-product");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RegisterEndpoint_SuccessfullyRegistersNewCustomerAndReturnsCreated()
    {
        var request = new RegisterRequest(
            Email: "newcustomer@example.com",
            Password: "SecurePassword123!",
            FirstName: "John",
            LastName: "Doe");

        var response = await client!.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("newcustomer@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task RegisterEndpointReturnsOnlyCustomerSafeFields()
    {
        var request = new RegisterRequest(
            Email: "customer@example.com",
            Password: "SecurePassword123!",
            FirstName: "Jane",
            LastName: "Smith");

        var response = await client!.PostAsJsonAsync("/api/auth/register", request);
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("normalizedEmail", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("isActive", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id", json);
        Assert.Contains("email", json);
        Assert.Contains("firstName", json);
        Assert.Contains("lastName", json);
        Assert.Contains("createdAt", json);
    }

    [Fact]
    public async Task RegisterEndpointWithDuplicateEmailReturnsConflict()
    {
        var request = new RegisterRequest(
            Email: "duplicate@example.com",
            Password: "SecurePassword123!",
            FirstName: "First",
            LastName: "User");

        // Register first customer
        var firstResponse = await client!.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Attempt to register with same email
        var secondResponse = await client!.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal("application/problem+json", secondResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RegisterEndpointWithDuplicateEmailIsCaseInsensitive()
    {
        var firstRequest = new RegisterRequest(
            Email: "casetest@example.com",
            Password: "SecurePassword123!",
            FirstName: "First",
            LastName: "User");

        var secondRequest = new RegisterRequest(
            Email: "CASETEST@EXAMPLE.COM", // Different casing, same normalized form
            Password: "AnotherPassword456!",
            FirstName: "Second",
            LastName: "User");

        // Register first customer
        var firstResponse = await client!.PostAsJsonAsync("/api/auth/register", firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Attempt to register with different casing (should still conflict)
        var secondResponse = await client!.PostAsJsonAsync("/api/auth/register", secondRequest);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task RegisterEndpointPersistsCustomerToDatabase()
    {
        var request = new RegisterRequest(
            Email: "persistent@example.com",
            Password: "SecurePassword123!",
            FirstName: "Persist",
            LastName: "Test");

        var response = await client!.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verify customer was persisted by attempting duplicate registration
        var duplicateResponse = await client!.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task LoginEndpoint_SuccessfullyAuthenticatesValidCredentials()
    {
        var registerRequest = new RegisterRequest(
            Email: "login@example.com",
            Password: "CorrectPassword123!",
            FirstName: "Login",
            LastName: "User");

        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "login@example.com",
            Password: "CorrectPassword123!");

        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("login@example.com", result.Email);
        Assert.Equal("Login", result.FirstName);
        Assert.Equal("User", result.LastName);
    }

    [Fact]
    public async Task LoginEndpointReturnsOnlyCustomerSafeFields()
    {
        var registerRequest = new RegisterRequest(
            Email: "safelogin@example.com",
            Password: "SecurePassword123!",
            FirstName: "Safe",
            LastName: "User");

        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "safelogin@example.com",
            Password: "SecurePassword123!");

        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("normalizedEmail", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("isActive", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginEndpointWithIncorrectPasswordReturnsUnauthorized()
    {
        var registerRequest = new RegisterRequest(
            Email: "wrongpass@example.com",
            Password: "CorrectPassword123!",
            FirstName: "Wrong",
            LastName: "Pass");

        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "wrongpass@example.com",
            Password: "IncorrectPassword456!");

        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginEndpointWithUnknownEmailReturnsUnauthorized()
    {
        var loginRequest = new LoginRequest(
            Email: "unknown@example.com",
            Password: "AnyPassword123!");

        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginEndpointWithCaseInsensitiveEmailSucceeds()
    {
        var registerRequest = new RegisterRequest(
            Email: "CaseInsensitive@Example.COM",
            Password: "CorrectPassword123!",
            FirstName: "Case",
            LastName: "Insensitive");

        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            Email: "caseinsensitive@example.com",
            Password: "CorrectPassword123!");

        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("CaseInsensitive@Example.COM", result.Email);
    }

    [Fact]
    public async Task LoginEndpointWithInactiveCustomerReturnsUnauthorized()
    {
        var registerRequest = new RegisterRequest(
            Email: "inactive@example.com",
            Password: "CorrectPassword123!",
            FirstName: "Inactive",
            LastName: "User");

        var registerResponse = await client!.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using var scope = factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

        var normalizedEmail = "INACTIVE@EXAMPLE.COM";
        var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.NormalizedEmail == normalizedEmail);

        Assert.NotNull(customer);

        customer.IsActive = false;

        dbContext.Customers.Update(customer);
        await dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest(
            Email: "inactive@example.com",
            Password: "CorrectPassword123!");

        var loginResponse = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        Assert.Equal("application/problem+json", loginResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginEndpoint_InvalidCredentialsDoNotRevealEmailExistence()
    {
        var registerRequest = new RegisterRequest(
            Email: "exists@example.com",
            Password: "CorrectPassword123!",
            FirstName: "Exists",
            LastName: "User");

        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);

        var unknownEmailRequest = new LoginRequest(
            Email: "unknown@example.com",
            Password: "AnyPassword123!");

        var unknownEmailResponse = await client!.PostAsJsonAsync("/api/auth/login", unknownEmailRequest);

        var wrongPasswordRequest = new LoginRequest(
            Email: "exists@example.com",
            Password: "WrongPassword123!");

        var wrongPasswordResponse = await client!.PostAsJsonAsync("/api/auth/login", wrongPasswordRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmailResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPasswordResponse.StatusCode);

        Assert.Equal("application/problem+json", unknownEmailResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal("application/problem+json", wrongPasswordResponse.Content.Headers.ContentType?.MediaType);
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();

        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

    }
}

#pragma warning restore CA1707

