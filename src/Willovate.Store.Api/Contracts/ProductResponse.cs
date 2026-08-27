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
    bool IsFeatured);
