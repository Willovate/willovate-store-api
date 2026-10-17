namespace Willovate.Store.Api.Services;

public sealed record GoogleUserPayload(
    string Subject,
    string Email,
    bool EmailVerified,
    string? GivenName,
    string? FamilyName,
    string? Name);

public interface IGoogleTokenValidator
{
    Task<GoogleUserPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
