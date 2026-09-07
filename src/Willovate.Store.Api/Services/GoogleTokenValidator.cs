using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Willovate.Store.Api.Configuration;

namespace Willovate.Store.Api.Services;

public sealed class GoogleTokenValidator(IOptions<GoogleOptions> googleOptions) : IGoogleTokenValidator
{
    public async Task<GoogleUserPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var clientId = googleOptions.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Google ClientId configuration is required.");
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            if (payload is null)
            {
                return null;
            }

            return new GoogleUserPayload(
                payload.Subject,
                payload.Email,
                payload.EmailVerified,
                payload.GivenName,
                payload.FamilyName,
                payload.Name);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
