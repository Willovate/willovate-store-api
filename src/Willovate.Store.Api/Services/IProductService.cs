using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Services;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetProductsAsync(
        string? search,
        string? category,
        bool? featured,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ProductResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken);
}
