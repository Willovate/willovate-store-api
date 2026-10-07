using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Willovate.Store.Api.Configuration;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

/// <summary>
/// Service for generating JWT access tokens with customer identity claims.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public string GenerateToken(Customer customer)
    {
        var jwtOptions = options.Value;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email),
            new Claim("name", $"{customer.FirstName} {customer.LastName}"),
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
