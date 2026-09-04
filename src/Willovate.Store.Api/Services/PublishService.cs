using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class PublishService(StoreDbContext dbContext) : IPublishService
{
    public async Task<PublicationStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        var status = await GetOrCreateStatusAsync(cancellationToken);

        return ToResponse(status);
    }

    public async Task<PublicationStatusResponse> PublishAsync(CancellationToken cancellationToken)
    {
        var status = await GetOrCreateStatusAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        status.IsPublished = true;
        status.PublishedAt = now;
        status.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(status);
    }

    private async Task<PublicationStatus> GetOrCreateStatusAsync(CancellationToken cancellationToken)
    {
        var status = await dbContext.PublicationStatuses.FirstOrDefaultAsync(cancellationToken);

        if (status is not null)
        {
            return status;
        }

        status = new PublicationStatus
        {
            Id = Guid.NewGuid(),
            IsPublished = false,
            PublishedAt = null,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        dbContext.PublicationStatuses.Add(status);
        await dbContext.SaveChangesAsync(cancellationToken);

        return status;
    }

    private static PublicationStatusResponse ToResponse(PublicationStatus status) => new(
        status.IsPublished,
        status.PublishedAt);
}
