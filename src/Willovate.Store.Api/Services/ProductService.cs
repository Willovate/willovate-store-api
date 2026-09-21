using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class ProductService(
    StoreDbContext dbContext,
    IWebHostEnvironment webHostEnvironment) : IProductService
{
    private static readonly HashSet<string> AllowedImageExtensions = [
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif", ".jfif", ".svg", ".bmp"
    ];

    // ── Public (storefront) ────────────────────────────────────────────────

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

        // Public endpoint: only show active products
        var query = dbContext.Products.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(p => p.SearchText.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var key = category.Trim().ToLowerInvariant();
            query = query.Where(p => p.CategoryKey == key);
        }

        if (featured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == featured.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        // Materialize entities first; ToResponse uses JsonSerializer which EF cannot translate.
        var entities = await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new(
            entities.Select(ToResponse).ToList(),
            page,
            pageSize,
            totalItems,
            (int)Math.Ceiling(totalItems / (double)pageSize));
    }

    public async Task<ProductResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var slugKey = slug.Trim().ToLowerInvariant();

        var product = await dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Slug == slugKey, cancellationToken);

        return product is null ? null : ToResponse(product);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await dbContext.Products
            .AsNoTracking()
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

    // ── Admin ──────────────────────────────────────────────────────────────

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304")]
    public async Task<PagedResponse<ProductResponse>> GetAdminProductsAsync(
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
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Admin: all products, no IsActive filter unless explicitly requested
        var query = dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(p => p.SearchText.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var key = category.Trim().ToLowerInvariant();
            query = query.Where(p => p.CategoryKey == key);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var isActive = string.Equals(status.Trim(), "active", StringComparison.OrdinalIgnoreCase);
            query = query.Where(p => p.IsActive == isActive);
        }

        // Product Types filter (multiple selection)
        if (!string.IsNullOrWhiteSpace(productTypes))
        {
            var types = productTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            if (types.Count > 0)
            {
                query = query.Where(p => p.ProductType != null && types.Contains(p.ProductType));
            }
        }

        // Tags filter (multiple selection)
        if (!string.IsNullOrWhiteSpace(tags))
        {
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            foreach (var tag in tagList)
            {
                var pattern = $"%{tag}%";
                query = query.Where(p => p.Tags != null && EF.Functions.ILike(p.Tags, pattern));
            }
        }

        // Collection filter (multiple selection)
        if (!string.IsNullOrWhiteSpace(collections))
        {
            var colList = collections.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            foreach (var col in colList)
            {
                var colLower = col.ToLowerInvariant();
                if (colLower == "featured products" || colLower == "featured")
                    query = query.Where(p => p.IsFeatured || (p.Tags != null && EF.Functions.ILike(p.Tags, "%featured%")));
                else if (colLower == "sale")
                    query = query.Where(p => (p.CompareAtPrice != null && p.CompareAtPrice > p.Price) || (p.Tags != null && EF.Functions.ILike(p.Tags, "%sale%")));
                else if (colLower == "new arrivals")
                    query = query.Where(p => p.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30) || (p.Tags != null && EF.Functions.ILike(p.Tags, "%new%")));
                else if (colLower == "best sellers" || colLower == "bestseller")
                    query = query.Where(p => p.Tags != null && (EF.Functions.ILike(p.Tags, "%bestseller%") || EF.Functions.ILike(p.Tags, "%trending%")));
                else if (colLower == "summer collection")
                    query = query.Where(p => (p.Tags != null && EF.Functions.ILike(p.Tags, "%summer%")) || EF.Functions.ILike(p.Name, "%summer%") || (p.Description != null && EF.Functions.ILike(p.Description, "%summer%")));
                else if (colLower == "winter collection")
                    query = query.Where(p => (p.Tags != null && EF.Functions.ILike(p.Tags, "%winter%")) || EF.Functions.ILike(p.Name, "%winter%") || (p.Description != null && EF.Functions.ILike(p.Description, "%winter%")));
                else
                    query = query.Where(p => (p.Tags != null && EF.Functions.ILike(p.Tags, $"%{col}%")) || EF.Functions.ILike(p.Name, $"%{col}%"));
            }
        }

        // Brands filter (multiple selection, exact tag match case-insensitively)
        if (!string.IsNullOrWhiteSpace(brands))
        {
            var brandList = brands.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            if (brandList.Count > 0)
            {
                var predicate = BuildBrandFilterPredicate(brandList);
                query = query.Where(predicate);
            }
        }

        // Stock Status filter
        if (!string.IsNullOrWhiteSpace(stockStatus) && stockStatus.ToLowerInvariant() != "all")
        {
            switch (stockStatus.ToLowerInvariant())
            {
                case "in-stock":
                    query = query.Where(p => p.StockQuantity > p.LowStockAlert);
                    break;
                case "low-stock":
                    query = query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.LowStockAlert);
                    break;
                case "out-of-stock":
                    query = query.Where(p => p.StockQuantity == 0);
                    break;
            }
        }

        // Variant filter
        if (!string.IsNullOrWhiteSpace(variantFilter) && variantFilter.ToLowerInvariant() != "all")
        {
            switch (variantFilter.ToLowerInvariant())
            {
                case "has-variants":
                    query = query.Where(p => p.Variants != null && p.Variants != "" && p.Variants != "[]");
                    break;
                case "no-variants":
                    query = query.Where(p => p.Variants == null || p.Variants == "" || p.Variants == "[]");
                    break;
            }
        }

        // Price filter
        if (minPrice.HasValue && minPrice.Value >= 0)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }
        if (maxPrice.HasValue && maxPrice.Value >= 0)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Updated Date filter
        if (!string.IsNullOrWhiteSpace(updatedRange) && updatedRange.ToLowerInvariant() != "any" && updatedRange.ToLowerInvariant() != "any time")
        {
            var now = DateTimeOffset.UtcNow;
            var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

            switch (updatedRange.ToLowerInvariant())
            {
                case "today":
                    query = query.Where(p => p.UpdatedAt >= todayStart);
                    break;
                case "yesterday":
                    var yesterdayStart = todayStart.AddDays(-1);
                    query = query.Where(p => p.UpdatedAt >= yesterdayStart && p.UpdatedAt < todayStart);
                    break;
                case "last7":
                case "last 7 days":
                    query = query.Where(p => p.UpdatedAt >= todayStart.AddDays(-7));
                    break;
                case "last30":
                case "last 30 days":
                    query = query.Where(p => p.UpdatedAt >= todayStart.AddDays(-30));
                    break;
                case "last90":
                case "last 90 days":
                    query = query.Where(p => p.UpdatedAt >= todayStart.AddDays(-90));
                    break;
                case "custom":
                case "custom date":
                    if (updatedFrom.HasValue)
                        query = query.Where(p => p.UpdatedAt >= updatedFrom.Value);
                    if (updatedTo.HasValue)
                        query = query.Where(p => p.UpdatedAt <= updatedTo.Value.AddDays(1));
                    break;
            }
        }

        // Sort — defaults to newest first
        query = sortBy?.Trim().ToLowerInvariant() switch
        {
            "oldest"     => query.OrderBy(p => p.CreatedAt),
            "name-asc"   => query.OrderBy(p => p.Name),
            "name-desc"  => query.OrderByDescending(p => p.Name),
            "price-asc"  => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            _            => query.OrderByDescending(p => p.CreatedAt),
        };

        var totalItems = await query.CountAsync(cancellationToken);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new(
            entities.Select(ToResponse).ToList(),
            page,
            pageSize,
            totalItems,
            (int)Math.Ceiling(totalItems / (double)pageSize));
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        return product is null ? null : ToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var slug = await GenerateUniqueSlugAsync(request.Name, excludeId: null, cancellationToken);

        var product = new Product
        {
            Id          = Guid.NewGuid(),
            Slug        = slug,
            Name        = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            SearchText  = BuildSearchText(request.Name, request.Description, request.Sku, request.Tags),
            Category    = request.Category.Trim(),
            CategoryKey = request.Category.Trim().ToLowerInvariant(),
            Price           = request.Price,
            CompareAtPrice  = request.CompareAtPrice,
            StockQuantity   = request.StockQuantity,
            VisualTheme     = string.IsNullOrWhiteSpace(request.VisualTheme) ? "sky" : request.VisualTheme.Trim(),
            IsFeatured      = request.IsFeatured,
            IsActive        = request.IsActive,
            Sku             = request.Sku?.Trim(),
            ProductType     = request.ProductType?.Trim(),
            Tags            = request.Tags?.Trim(),
            LowStockAlert   = request.LowStockAlert,
            ImageUrls       = "[]",
            Variants        = request.Variants,
            CreatedAt       = now,
            UpdatedAt       = now,
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductResponse?> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FindAsync(new object?[] { id }, cancellationToken);

        if (product is null) return null;

        // Regenerate slug only when the name has actually changed
        if (!string.Equals(product.Name, request.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            product.Slug = await GenerateUniqueSlugAsync(request.Name, excludeId: id, cancellationToken);
        }

        product.Name        = request.Name.Trim();
        product.Description = request.Description?.Trim() ?? string.Empty;
        product.SearchText  = BuildSearchText(request.Name, request.Description, request.Sku, request.Tags);
        product.Category    = request.Category.Trim();
        product.CategoryKey = request.Category.Trim().ToLowerInvariant();
        product.Price           = request.Price;
        product.CompareAtPrice  = request.CompareAtPrice;
        product.StockQuantity   = request.StockQuantity;
        product.VisualTheme     = string.IsNullOrWhiteSpace(request.VisualTheme)
                                    ? product.VisualTheme
                                    : request.VisualTheme.Trim();
        product.IsFeatured      = request.IsFeatured;
        product.IsActive        = request.IsActive;
        product.Sku             = request.Sku?.Trim();
        product.ProductType     = request.ProductType?.Trim();
        product.Tags            = request.Tags?.Trim();
        product.LowStockAlert   = request.LowStockAlert;
        product.Variants        = request.Variants;
        product.UpdatedAt       = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FindAsync(new object?[] { id }, cancellationToken);

        if (product is null) return false;

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ── Image management ───────────────────────────────────────────────────
    // All file I/O is contained here. To switch to cloud storage, replace
    // this method body and update SaveImageAsync on the interface accordingly.

    public async Task<string?> SaveImageAsync(
        Stream imageStream,
        string fileName,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension)) return null;

        // WebRootPath may be null in test/CI environments; fall back gracefully.
        var webRoot = webHostEnvironment.WebRootPath
            ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");

        var uploadsDir = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(uploadsDir);

        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var filePath   = Path.Combine(uploadsDir, uniqueName);

        await using var fileStream = File.Create(filePath);
        await imageStream.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/products/{uniqueName}";
    }

    public async Task<ProductResponse?> UpdateImagesAsync(
        Guid id,
        IReadOnlyList<string> imageUrls,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FindAsync(new object?[] { id }, cancellationToken);

        if (product is null) return null;

        product.ImageUrls = JsonSerializer.Serialize(imageUrls);
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(product);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var baseSlug = GenerateSlug(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "product";

        var slug    = baseSlug;
        var counter = 2;

        while (await dbContext.Products.AnyAsync(
            p => p.Slug == slug && (excludeId == null || p.Id != excludeId),
            cancellationToken))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        return slug;
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant().Replace(' ', '-');
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", string.Empty);
        slug = Regex.Replace(slug, @"-{2,}", "-");
        return slug.Trim('-');
    }

    private static string BuildSearchText(
        string name,
        string? description,
        string? sku,
        string? tags) =>
        $"{name} {description} {sku} {tags}".Trim().ToLowerInvariant();

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
        product.IsFeatured,
        // Admin fields
        product.IsActive,
        product.Sku,
        product.ProductType,
        product.Tags,
        product.LowStockAlert,
        DeserializeImageUrls(product.ImageUrls),
        product.Variants,
        product.UpdatedAt);

    private static List<string> DeserializeImageUrls(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static Expression<Func<Product, bool>> BuildBrandFilterPredicate(List<string> brandList)
    {
        var param = Expression.Parameter(typeof(Product), "p");
        var tagsProp = Expression.Property(param, nameof(Product.Tags));
        var notNull = Expression.NotEqual(tagsProp, Expression.Constant(null, typeof(string)));

        var ilikeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
            nameof(NpgsqlDbFunctionsExtensions.ILike),
            [typeof(DbFunctions), typeof(string), typeof(string)]
        );

        var dbFunctionsExpr = Expression.Property(null, typeof(EF), nameof(EF.Functions));

        Expression? combined = null;

        foreach (var b in brandList)
        {
            var patterns = new[]
            {
                b,
                $"{b},%",
                $"%, {b}",
                $"%,{b}",
                $"%, {b},%",
                $"%,{b},%"
            };

            Expression? bCombined = null;
            foreach (var pat in patterns)
            {
                var call = Expression.Call(ilikeMethod!, dbFunctionsExpr, tagsProp, Expression.Constant(pat));
                bCombined = bCombined == null ? call : Expression.OrElse(bCombined, call);
            }

            combined = combined == null ? bCombined : Expression.OrElse(combined, bCombined!);
        }

        var body = Expression.AndAlso(notNull, combined!);
        return Expression.Lambda<Func<Product, bool>>(body, param);
    }
}
