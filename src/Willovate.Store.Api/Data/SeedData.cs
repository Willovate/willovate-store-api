using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Data;

public static class SeedData
{
    public static async Task InitialiseAsync(StoreDbContext dbContext)
    {
        if (await dbContext.Products.AnyAsync())
        {
            return;
        }

        var createdAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        dbContext.Products.AddRange(
            Product("cloud-linen-shirt", "Cloud Linen Shirt", "Relaxed tailoring in breathable European linen.", "Apparel", 2499, 3199, 18, "sky", true, createdAt),
            Product("orbit-desk-lamp", "Orbit Desk Lamp", "Warm, focused light with a sculptural matte finish.", "Home", 3899, null, 9, "sun", true, createdAt),
            Product("daybreak-tote", "Daybreak Tote", "A spacious everyday carry made from recycled canvas.", "Accessories", 1799, 2199, 24, "coral", true, createdAt),
            Product("stillness-candle", "Stillness Candle", "Cedar, bergamot and rain with a clean soy wax burn.", "Home", 899, null, 34, "lavender", false, createdAt),
            Product("studio-wireless-headphones", "Studio Wireless Headphones", "Balanced sound, soft-touch comfort and 40-hour battery life.", "Tech", 6999, 7999, 11, "ink", true, createdAt),
            Product("everyday-sneakers", "Everyday Sneakers", "Low-profile comfort designed for long city walks.", "Apparel", 4299, null, 16, "mint", false, createdAt),
            Product("field-notebook-set", "Field Notebook Set", "Three lay-flat notebooks with dot-grid recycled paper.", "Stationery", 599, 749, 42, "sand", false, createdAt),
            Product("arc-water-bottle", "Arc Water Bottle", "Double-wall stainless steel that stays cold for 24 hours.", "Accessories", 1299, null, 27, "ocean", false, createdAt));

        await dbContext.SaveChangesAsync();
    }

    private static Product Product(
        string slug,
        string name,
        string description,
        string category,
        decimal price,
        decimal? compareAtPrice,
        int stockQuantity,
        string visualTheme,
        bool isFeatured,
        DateTimeOffset createdAt) => new()
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Name = name,
            Description = description,
            SearchText = $"{name} {description}".ToLowerInvariant(),
            Category = category,
            CategoryKey = category.ToLowerInvariant(),
            Price = price,
            CompareAtPrice = compareAtPrice,
            StockQuantity = stockQuantity,
            VisualTheme = visualTheme,
            IsFeatured = isFeatured,
            CreatedAt = createdAt
        };
}
