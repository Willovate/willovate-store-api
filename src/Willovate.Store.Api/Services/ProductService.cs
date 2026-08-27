using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class ProductService(StoreDbContext dbContext) : IProductService
{
    public async Task<PagedResponse<ProductResponse>> GetProductsAsync(
        string? search,
        string? category,
        bool? featured,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(product => product.SearchText.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var selectedCategory = category.Trim().ToLowerInvariant();
            query = query.Where(product => product.CategoryKey == selectedCategory);
        }

        if (featured.HasValue)
        {
            query = query.Where(product => product.IsFeatured == featured.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var products = await query
            .OrderByDescending(product => product.IsFeatured)
            .ThenBy(product => product.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => ToResponse(product))
            .ToListAsync(cancellationToken);

        return new(
            products,
            page,
            pageSize,
            totalItems,
            (int)Math.Ceiling(totalItems / (double)pageSize));
    }

    public Task<ProductResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var slugKey = slug.Trim().ToLowerInvariant();

        return
        dbContext.Products
            .AsNoTracking()
            .Where(product => product.Slug == slugKey)
            .Select(product => ToResponse(product))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await dbContext.Products
            .AsNoTracking()
            .Select(product => product.Category)
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync(cancellationToken);

    private static ProductResponse ToResponse(Product product) => new(
        product.Id,
        product.Slug,
        product.Name,
        product.Description,
        product.Category,
        product.Price,
        product.CompareAtPrice,
        product.StockQuantity,
        product.VisualTheme,
        product.IsFeatured);
}
