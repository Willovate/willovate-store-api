namespace Willovate.Store.Api.Models;

public sealed class Product
{
    public Guid Id { get; init; }
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string SearchText { get; set; }
    public required string Category { get; set; }
    public required string CategoryKey { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public required string VisualTheme { get; set; }
    public bool IsFeatured { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
