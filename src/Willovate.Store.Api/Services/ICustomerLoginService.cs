using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Services;

public interface ICustomerLoginService
{
    Task<CustomerResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}
