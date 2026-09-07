#pragma warning disable CA1707

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;

namespace Willovate.Store.Api.Tests;

public sealed class StoreApiTests : IAsyncLifetime
{
    private const string JwtSecret = "test-secret-key-must-be-at-least-32-characters-long-for-hs256";
    private const string JwtIssuer = "test-issuer";
    private const string JwtAudience = "test-audience";
    private WebApplicationFactory<Program>? factory;
    private HttpClient? client;

    public Task InitializeAsync()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Secret", JwtSecret);
            builder.UseSetting("Jwt:Issuer", JwtIssuer);
            builder.UseSetting("Jwt:Audience", JwtAudience);
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
        var request = new RegisterRequest("newcustomer@example.com", "SecurePassword123!", "John", "Doe");
        var response = await client!.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("newcustomer@example.com", result.Customer.Email);
        Assert.Equal("John", result.Customer.FirstName);
        Assert.Equal("Doe", result.Customer.LastName);
        Assert.NotEqual(Guid.Empty, result.Customer.Id);
    }

    [Fact]
    public async Task RegisterEndpointReturnsValidSignedJwtForCustomer()
    {
        var request = new RegisterRequest("jwtregistration@example.com", "SecurePassword123!", "Jwt", "Customer");
        var response = await client!.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = JwtIssuer,
            ValidateAudience = true,
            ValidAudience = JwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };
        var principal = tokenHandler.ValidateToken(result.AccessToken, validationParameters, out _);

        Assert.Equal(result.Customer.Id.ToString(), principal.FindFirst("sub")?.Value);
        Assert.Equal(JwtIssuer, principal.FindFirst("iss")?.Value);
        Assert.Equal(JwtAudience, principal.FindFirst("aud")?.Value);
    }

    [Fact]
    public async Task RegisterEndpointReturnsOnlyCustomerSafeFields()
    {
        var request = new RegisterRequest("customer@example.com", "SecurePassword123!", "Jane", "Smith");
        var response = await client!.PostAsJsonAsync("/api/auth/register", request);
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("normalizedEmail", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("isActive", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("accessToken", json);
        Assert.Contains("customer", json);
        Assert.Contains("id", json);
        Assert.Contains("email", json);
        Assert.Contains("firstName", json);
        Assert.Contains("lastName", json);
        Assert.Contains("createdAt", json);
    }

    [Fact]
    public async Task RegisterEndpointWithDuplicateEmailReturnsConflict()
    {
        var request = new RegisterRequest("duplicate@example.com", "SecurePassword123!", "First", "User");
        var firstResponse = await client!.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await client!.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal("application/problem+json", secondResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RegisterEndpointWithDuplicateEmailIsCaseInsensitive()
    {
        var firstRequest = new RegisterRequest("casetest@example.com", "SecurePassword123!", "First", "User");
        var secondRequest = new RegisterRequest("CASETEST@EXAMPLE.COM", "AnotherPassword456!", "Second", "User");
        var firstResponse = await client!.PostAsJsonAsync("/api/auth/register", firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await client!.PostAsJsonAsync("/api/auth/register", secondRequest);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task RegisterEndpointPersistsCustomerToDatabase()
    {
        var request = new RegisterRequest("persistent@example.com", "SecurePassword123!", "Persist", "Test");
        var response = await client!.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);

        var duplicateResponse = await client!.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task LoginEndpoint_SuccessfullyAuthenticatesValidCredentials()
    {
        var registerRequest = new RegisterRequest("login@example.com", "CorrectPassword123!", "Login", "User");
        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);
        var loginRequest = new LoginRequest("login@example.com", "CorrectPassword123!");
        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("login@example.com", result.Customer.Email);
        Assert.Equal("Login", result.Customer.FirstName);
        Assert.Equal("User", result.Customer.LastName);
    }

    [Fact]
    public async Task LoginEndpointReturnsValidSignedJwtForCustomer()
    {
        var registerRequest = new RegisterRequest("jwtlogin@example.com", "SecurePassword123!", "Jwt", "Customer");
        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);
        var loginRequest = new LoginRequest("jwtlogin@example.com", "SecurePassword123!");
        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = JwtIssuer,
            ValidateAudience = true,
            ValidAudience = JwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };
        var principal = tokenHandler.ValidateToken(result.AccessToken, validationParameters, out _);

        Assert.Equal(result.Customer.Id.ToString(), principal.FindFirst("sub")?.Value);
        Assert.Equal(result.Customer.Email, principal.FindFirst("email")?.Value);
        Assert.Equal(JwtIssuer, principal.FindFirst("iss")?.Value);
        Assert.Equal(JwtAudience, principal.FindFirst("aud")?.Value);
    }

    [Fact]
    public async Task LoginEndpointReturnsOnlyCustomerSafeFields()
    {
        var registerRequest = new RegisterRequest("safelogin@example.com", "SecurePassword123!", "Safe", "User");
        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);
        var loginRequest = new LoginRequest("safelogin@example.com", "SecurePassword123!");
        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("normalizedEmail", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("isActive", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("accessToken", json);
        Assert.Contains("customer", json);
    }

    [Fact]
    public async Task LoginEndpointWithIncorrectPasswordReturnsUnauthorized()
    {
        var registerRequest = new RegisterRequest("wrongpass@example.com", "CorrectPassword123!", "Wrong", "Pass");
        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);
        var loginRequest = new LoginRequest("wrongpass@example.com", "IncorrectPassword456!");
        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginEndpointWithUnknownEmailReturnsUnauthorized()
    {
        var response = await client!.PostAsJsonAsync("/api/auth/login", new LoginRequest("unknown@example.com", "AnyPassword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginEndpointWithCaseInsensitiveEmailSucceeds()
    {
        var registerRequest = new RegisterRequest("CaseInsensitive@Example.COM", "CorrectPassword123!", "Case", "Insensitive");
        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);
        var loginRequest = new LoginRequest("caseinsensitive@example.com", "CorrectPassword123!");
        var response = await client!.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("CaseInsensitive@Example.COM", result.Customer.Email);
    }

    [Fact]
    public async Task LoginEndpointWithInactiveCustomerReturnsUnauthorized()
    {
        var registerRequest = new RegisterRequest("inactive@example.com", "CorrectPassword123!", "Inactive", "User");
        var registerResponse = await client!.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using var scope = factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.NormalizedEmail == "INACTIVE@EXAMPLE.COM");

        Assert.NotNull(customer);
        customer.IsActive = false;
        dbContext.Customers.Update(customer);
        await dbContext.SaveChangesAsync();

        var loginResponse = await client!.PostAsJsonAsync("/api/auth/login", new LoginRequest("inactive@example.com", "CorrectPassword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        Assert.Equal("application/problem+json", loginResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LoginEndpoint_InvalidCredentialsDoNotRevealEmailExistence()
    {
        var registerRequest = new RegisterRequest("exists@example.com", "CorrectPassword123!", "Exists", "User");
        await client!.PostAsJsonAsync("/api/auth/register", registerRequest);
        var unknownEmailResponse = await client!.PostAsJsonAsync("/api/auth/login", new LoginRequest("unknown@example.com", "AnyPassword123!"));
        var wrongPasswordResponse = await client!.PostAsJsonAsync("/api/auth/login", new LoginRequest("exists@example.com", "WrongPassword123!"));

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
