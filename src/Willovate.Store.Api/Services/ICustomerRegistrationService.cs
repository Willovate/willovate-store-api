using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Services;

public interface ICustomerRegistrationService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);
}
