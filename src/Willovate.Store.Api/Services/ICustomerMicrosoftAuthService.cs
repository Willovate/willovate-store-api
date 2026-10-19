using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Services;

public interface ICustomerMicrosoftAuthService
{
    Task<AuthResponse> AuthenticateMicrosoftUserAsync(
        MicrosoftAuthRequest request,
        CancellationToken cancellationToken = default);
}
