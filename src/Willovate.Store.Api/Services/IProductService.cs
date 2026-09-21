using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Services;

public interface IProductService
{
    // ── Public (storefront) ────────────────────────────────────────────────
    // These methods return only IsActive = true products.

    Task<PagedResponse<ProductResponse>> GetProductsAsync(
        string? search,
        string? category,
        bool? featured,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ProductResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken);

    // ── Admin ──────────────────────────────────────────────────────────────
    // These methods operate on all products regardless of IsActive status.
    // TODO: Protect with an authorization policy when auth is added.

    Task<PagedResponse<ProductResponse>> GetAdminProductsAsync(
        string? search,
        string? category,
        string? status,
        string? sortBy,
        int page,
        int pageSize,
        string? productTypes = null,
        string? tags = null,
        string? collections = null,
        string? brands = null,
        string? stockStatus = null,
        string? variantFilter = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? updatedRange = null,
        DateTimeOffset? updatedFrom = null,
        DateTimeOffset? updatedTo = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a single product by ID regardless of IsActive status (admin use).</summary>
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    // ── Image management ───────────────────────────────────────────────────
    // Isolated here so the storage backend (local files, cloud, CDN) can be
    // swapped by replacing only these two methods and their implementation.

    Task<string?> SaveImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken);

    Task<ProductResponse?> UpdateImagesAsync(Guid id, IReadOnlyList<string> imageUrls, CancellationToken cancellationToken);
}
