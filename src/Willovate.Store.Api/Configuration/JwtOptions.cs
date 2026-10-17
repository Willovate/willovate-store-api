namespace Willovate.Store.Api.Configuration;

/// <summary>
/// Configuration options for JWT token generation and validation.
/// Bind from "Jwt" section in appsettings.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// The signing key used to create and validate JWT signatures.
    /// Must be at least 32 characters for HS256.
    /// </summary>
    public required string Secret { get; set; }

    /// <summary>
    /// The issuer claim (iss) for JWT tokens.
    /// </summary>
    public required string Issuer { get; set; }

    /// <summary>
    /// The audience claim (aud) for JWT tokens.
    /// </summary>
    public required string Audience { get; set; }

    /// <summary>
    /// Token expiration lifetime in minutes.
    /// </summary>
    public required int ExpirationMinutes { get; set; }
}
