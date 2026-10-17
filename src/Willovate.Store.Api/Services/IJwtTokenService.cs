using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

/// <summary>
/// Service for generating JWT access tokens for authenticated customers.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the given customer.
    /// </summary>
    /// <param name="customer">The customer to create a token for.</param>
    /// <returns>A signed JWT token as a string.</returns>
    string GenerateToken(Customer customer);
}
