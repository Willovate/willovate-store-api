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

    // Admin fields added for the Products admin feature
    /// <summary>Controls storefront visibility. Only active products appear on the public catalog.</summary>
    public bool IsActive { get; set; } = true;

    public string? Sku { get; set; }
    public string? ProductType { get; set; }

    /// <summary>Comma-separated tags, e.g. "summer,cotton,new".</summary>
    public string? Tags { get; set; }

    /// <summary>Stock quantity threshold below which the product is considered low-stock.</summary>
    public int LowStockAlert { get; set; } = 5;

    /// <summary>JSON array of up to 5 relative image paths, e.g. ["/uploads/products/abc.jpg"].</summary>
    public string ImageUrls { get; set; } = "[]";

    /// <summary>JSON array of variant objects (admin-only; not surfaced to the storefront in this phase).</summary>
    public string? Variants { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
