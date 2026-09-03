using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Services;

public interface IWebsiteService
{
    Task<WebsiteResponse?> GetWebsiteAsync(Guid websiteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<WebsiteResponse>> GetWebsitesAsync(CancellationToken cancellationToken);
    Task<WebsiteResponse> CreateWebsiteAsync(CreateWebsiteRequest request, CancellationToken cancellationToken);
    Task<WebsiteResponse> UpdateWebsiteAsync(Guid websiteId, UpdateWebsiteRequest request, CancellationToken cancellationToken);
    Task DeleteWebsiteAsync(Guid websiteId, CancellationToken cancellationToken);
}

public interface IPageService
{
    Task<PageResponse?> GetPageAsync(Guid pageId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PageResponse>> GetPagesByWebsiteAsync(Guid websiteId, CancellationToken cancellationToken);
    Task<PageResponse> CreatePageAsync(Guid websiteId, CreatePageRequest request, CancellationToken cancellationToken);
    Task<PageResponse> UpdatePageAsync(Guid pageId, UpdatePageRequest request, CancellationToken cancellationToken);
    Task DeletePageAsync(Guid pageId, CancellationToken cancellationToken);
    Task<PageResponse> GetPageBySlugAsync(Guid websiteId, string slug, CancellationToken cancellationToken);
}

public interface IPageElementService
{
    Task<PageElementResponse?> GetElementAsync(Guid elementId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PageElementResponse>> GetElementsByPageAsync(Guid pageId, CancellationToken cancellationToken);
    Task<PageElementResponse> CreateElementAsync(Guid pageId, CreatePageElementRequest request, CancellationToken cancellationToken);
    Task<PageElementResponse> UpdateElementAsync(Guid elementId, UpdatePageElementRequest request, CancellationToken cancellationToken);
    Task DeleteElementAsync(Guid elementId, CancellationToken cancellationToken);
}
