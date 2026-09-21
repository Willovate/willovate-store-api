using System.ComponentModel.DataAnnotations;

namespace Willovate.Store.Api.Contracts;

public sealed record CreateProductRequest(
    [Required, MaxLength(180)] string Name,
    [MaxLength(50_000)] string? Description,
    [Required, MaxLength(300)] string Category,
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    decimal Price,
    decimal? CompareAtPrice,
    [Range(0, int.MaxValue)] int StockQuantity,
    [MaxLength(40)] string VisualTheme,
    bool IsFeatured,
    bool IsActive,
    [MaxLength(120)] string? Sku,
    [MaxLength(80)] string? ProductType,
    [MaxLength(500)] string? Tags,
    [Range(0, int.MaxValue)] int LowStockAlert,
    /// <summary>JSON array of variant objects. Validated in the service layer.</summary>
    [MaxLength(100_000)] string? Variants);
