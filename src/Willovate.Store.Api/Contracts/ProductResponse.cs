namespace Willovate.Store.Api.Contracts;

public sealed record ProductResponse(
    Guid Id,
    string Slug,
    string Name,
    string Description,
    string Category,
    decimal Price,
    decimal? CompareAtPrice,
    int StockQuantity,
    string VisualTheme,
    bool IsFeatured,
    // Admin fields
    bool IsActive,
    string? Sku,
    string? ProductType,
    string? Tags,
    int LowStockAlert,
    IReadOnlyList<string> ImageUrls,
    string? Variants,
    DateTimeOffset UpdatedAt);
