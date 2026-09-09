namespace Willovate.Store.Api.Services;

public sealed record MicrosoftUserPayload(
    string Subject,
    string Email,
    string? GivenName,
    string? FamilyName,
    string? Name);

public interface IMicrosoftTokenValidator
{
    Task<MicrosoftUserPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
