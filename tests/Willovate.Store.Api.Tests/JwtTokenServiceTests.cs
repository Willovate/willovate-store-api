#pragma warning disable CA1707

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Willovate.Store.Api.Configuration;
using Willovate.Store.Api.Models;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Tests;

public sealed class JwtTokenServiceTests
{
    private readonly JwtOptions jwtOptions = new()
    {
        Secret = "test-secret-key-must-be-at-least-32-characters-long-for-hs256",
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpirationMinutes = 60
    };

    private readonly JwtTokenService jwtTokenService;

    public JwtTokenServiceTests()
    {
        var options = Options.Create(jwtOptions);
        jwtTokenService = new JwtTokenService(options);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwt()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(token.Contains('.'), "Token should contain JWT format with dots");
    }

    [Fact]
    public void GenerateToken_IncludesCustomerIdInSubClaim()
    {
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);
        var claims = ValidateAndGetClaims(token);

        Assert.NotNull(claims);
        var subClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.NotNull(subClaim);
        Assert.Equal(customerId.ToString(), subClaim.Value);
    }

    [Fact]
    public void GenerateToken_IncludesEmailClaim()
    {
        const string email = "john.doe@example.com";
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);
        var claims = ValidateAndGetClaims(token);

        Assert.NotNull(claims);
        var emailClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(email, emailClaim.Value);
    }

    [Fact]
    public void GenerateToken_IncludesFullNameInNameClaim()
    {
        const string firstName = "John";
        const string lastName = "Doe";
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PasswordHash = "hashed-password",
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);
        var claims = ValidateAndGetClaims(token);

        Assert.NotNull(claims);
        var nameClaim = claims.FirstOrDefault(c => c.Type == "name");
        Assert.NotNull(nameClaim);
        Assert.Equal("John Doe", nameClaim.Value);
    }

    [Fact]
    public void GenerateToken_IncludesIssuerClaim()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        Assert.NotNull(jwtToken);
        Assert.Equal("test-issuer", jwtToken.Issuer);
    }

    [Fact]
    public void GenerateToken_IncludesAudienceClaim()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        Assert.NotNull(jwtToken);
        Assert.Single(jwtToken.Audiences);
        Assert.Contains("test-audience", jwtToken.Audiences);
    }

    [Fact]
    public void GenerateToken_IncludesExpirationClaim()
    {
        var beforeGeneration = DateTime.UtcNow;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        var afterGeneration = DateTime.UtcNow;

        Assert.NotNull(jwtToken);
        var expectedExpiration = beforeGeneration.AddMinutes(60);
        var actualExpiration = jwtToken.ValidTo;

        // Allow 5 second tolerance for test execution time
        Assert.True(
            actualExpiration >= expectedExpiration.AddSeconds(-5) &&
            actualExpiration <= afterGeneration.AddMinutes(60).AddSeconds(5),
            $"Expiration should be approximately 60 minutes from now. Expected: ~{expectedExpiration}, Actual: {actualExpiration}");
    }

    [Fact]
    public void GenerateToken_CanBeValidated()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        handler.MapInboundClaims = false;
        var validationResult = handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

        Assert.NotNull(validationResult);
        Assert.NotNull(validatedToken);
        Assert.Equal(validationResult.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, customer.Id.ToString());
    }

    [Fact]
    public void GenerateToken_InvalidSigningKeyFailsValidation()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PasswordHash = "hashed-password",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        var token = jwtTokenService.GenerateToken(customer);

        var wrongSecret = "wrong-secret-key-must-be-at-least-32-characters-long-for-hs256!";
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(wrongSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        var exception = Assert.ThrowsAny<SecurityTokenException>(() =>
            handler.ValidateToken(token, tokenValidationParameters, out _));

        Assert.NotNull(exception);
    }

    private List<Claim> ValidateAndGetClaims(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = false, // Don't validate lifetime for test claims extraction
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        handler.MapInboundClaims = false;
        var principal = handler.ValidateToken(token, tokenValidationParameters, out _);

        return principal.Claims.ToList();
    }
}

#pragma warning restore CA1707
