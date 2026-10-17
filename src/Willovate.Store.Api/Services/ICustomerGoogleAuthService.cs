using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Services;

public interface ICustomerGoogleAuthService
{
    Task<AuthResponse> AuthenticateGoogleUserAsync(
        GoogleAuthRequest request,
        CancellationToken cancellationToken);
}
