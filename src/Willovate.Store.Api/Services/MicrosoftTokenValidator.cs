using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Willovate.Store.Api.Configuration;

namespace Willovate.Store.Api.Services;

public sealed class MicrosoftTokenValidator(
    IOptions<MicrosoftOptions> microsoftOptions,
    IConfigurationManager<OpenIdConnectConfiguration>? configurationManager = null) : IMicrosoftTokenValidator
{
    private IConfigurationManager<OpenIdConnectConfiguration>? _configurationManager = configurationManager;
    private readonly JwtSecurityTokenHandler tokenHandler = new();

    public async Task<MicrosoftUserPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var clientId = microsoftOptions.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Microsoft ClientId configuration is required.");
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        try
        {
            var tenantId = string.IsNullOrWhiteSpace(microsoftOptions.Value.TenantId)
                ? "common"
                : microsoftOptions.Value.TenantId;

            _configurationManager ??= new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            var oidcConfig = await _configurationManager.GetConfigurationAsync(cancellationToken);

            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateIssuer = true,
                IssuerValidator = (issuer, token, parameters) => ValidateIssuer(issuer, token, parameters, tenantId),
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = oidcConfig.SigningKeys,
                ValidateLifetime = true
            };

            var principal = tokenHandler.ValidateToken(idToken, validationParameters, out _);

            var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;

            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst("email")?.Value
                ?? principal.FindFirst("preferred_username")?.Value
                ?? principal.FindFirst(ClaimTypes.Upn)?.Value
                ?? principal.FindFirst("upn")?.Value;

            if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var givenName = principal.FindFirst(ClaimTypes.GivenName)?.Value ?? principal.FindFirst("given_name")?.Value;
            var familyName = principal.FindFirst(ClaimTypes.Surname)?.Value ?? principal.FindFirst("family_name")?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.FindFirst("name")?.Value;

            return new MicrosoftUserPayload(sub, email, givenName, familyName, name);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static string ValidateIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters, string configuredTenantId)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new SecurityTokenInvalidIssuerException("Issuer cannot be empty.");
        }

        if (!IsMultiTenant(configuredTenantId))
        {
            var expectedV2Issuer = $"https://login.microsoftonline.com/{configuredTenantId}/v2.0";
            var expectedV1Issuer = $"https://sts.windows.net/{configuredTenantId}/";

            if (string.Equals(issuer, expectedV2Issuer, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(issuer, expectedV1Issuer, StringComparison.OrdinalIgnoreCase))
            {
                return issuer;
            }

            throw new SecurityTokenInvalidIssuerException($"Issuer '{issuer}' does not match configured tenant '{configuredTenantId}'.");
        }

        if (IsValidMicrosoftIssuerFormat(issuer))
        {
            return issuer;
        }

        throw new SecurityTokenInvalidIssuerException($"Issuer '{issuer}' is not a valid Microsoft issuer.");
    }

    private static bool IsMultiTenant(string tenantId) =>
        tenantId.Equals("common", StringComparison.OrdinalIgnoreCase) ||
        tenantId.Equals("organizations", StringComparison.OrdinalIgnoreCase) ||
        tenantId.Equals("consumers", StringComparison.OrdinalIgnoreCase);

    private static bool IsValidMicrosoftIssuerFormat(string issuer)
    {
        if (issuer.StartsWith("https://login.microsoftonline.com/", StringComparison.OrdinalIgnoreCase) &&
            issuer.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase))
        {
            var tenantPart = issuer.Substring("https://login.microsoftonline.com/".Length, issuer.Length - "https://login.microsoftonline.com/".Length - "/v2.0".Length);
            return !string.IsNullOrWhiteSpace(tenantPart) && Guid.TryParse(tenantPart, out _);
        }

        if (issuer.StartsWith("https://sts.windows.net/", StringComparison.OrdinalIgnoreCase))
        {
            var tenantPart = issuer["https://sts.windows.net/".Length..].TrimEnd('/');
            return !string.IsNullOrWhiteSpace(tenantPart) && Guid.TryParse(tenantPart, out _);
        }

        return false;
    }
}
