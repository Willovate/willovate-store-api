using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Services;

public interface IPublishService
{
    Task<PublicationStatusResponse> GetStatusAsync(CancellationToken cancellationToken);

    Task<PublicationStatusResponse> PublishAsync(CancellationToken cancellationToken);
}
