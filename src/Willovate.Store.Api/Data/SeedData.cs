using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Data;

public static class SeedData
{
    public static async Task InitialiseAsync(StoreDbContext dbContext)
    {
        var createdAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                Product("cloud-linen-shirt", "Cloud Linen Shirt", "Relaxed tailoring in breathable European linen.", "Apparel", 2499, 3199, 18, "sky", true, createdAt),
                Product("orbit-desk-lamp", "Orbit Desk Lamp", "Warm, focused light with a sculptural matte finish.", "Home", 3899, null, 9, "sun", true, createdAt),
                Product("daybreak-tote", "Daybreak Tote", "A spacious everyday carry made from recycled canvas.", "Accessories", 1799, 2199, 24, "coral", true, createdAt),
                Product("stillness-candle", "Stillness Candle", "Cedar, bergamot and rain with a clean soy wax burn.", "Home", 899, null, 34, "lavender", false, createdAt),
                Product("studio-wireless-headphones", "Studio Wireless Headphones", "Balanced sound, soft-touch comfort and 40-hour battery life.", "Tech", 6999, 7999, 11, "ink", true, createdAt),
                Product("everyday-sneakers", "Everyday Sneakers", "Low-profile comfort designed for long city walks.", "Apparel", 4299, null, 16, "mint", false, createdAt),
                Product("field-notebook-set", "Field Notebook Set", "Three lay-flat notebooks with dot-grid recycled paper.", "Stationery", 599, 749, 42, "sand", false, createdAt),
                Product("arc-water-bottle", "Arc Water Bottle", "Double-wall stainless steel that stays cold for 24 hours.", "Accessories", 1299, null, 27, "ocean", false, createdAt));
        }

        if (!await dbContext.Templates.AnyAsync())
        {
            dbContext.Templates.AddRange(
                Template("mino-store", "Mino Store", "clothing-store", ["Minimal", "Modern"], "Minimal fashion store for everyday style.", "https://images.unsplash.com/photo-1441986300917-64674bd600d8?auto=format&fit=crop&w=800&q=80", "https://images.unsplash.com/photo-1441986300917-64674bd600d8?auto=format&fit=crop&w=1600&q=80", 100, createdAt),
                Template("layer-fashion", "Layer Fashion", "clothing-store", ["Casual", "Modern"], "Modern clothing & apparel store.", "https://images.unsplash.com/photo-1472851294608-062f824d29cc?auto=format&fit=crop&w=800&q=80", "https://images.unsplash.com/photo-1472851294608-062f824d29cc?auto=format&fit=crop&w=1600&q=80", 95, createdAt),
                Template("urban-thread", "Urban Thread", "clothing-store", ["Streetwear"], "Streetwear store for the bold and unique.", "https://images.unsplash.com/photo-1552374196-1ab2a1c593e8?auto=format&fit=crop&w=800&q=80", "https://images.unsplash.com/photo-1552374196-1ab2a1c593e8?auto=format&fit=crop&w=1600&q=80", 90, createdAt),
                Template("velora", "Velora", "clothing-store", ["Luxury", "Boutique"], "Luxury fashion boutique for timeless elegance.", "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?auto=format&fit=crop&w=800&q=80", "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?auto=format&fit=crop&w=1600&q=80", 88, createdAt),
                Template("luna-boutique", "Luna Boutique", "clothing-store", ["Boutique", "Luxury"], "Elegant women's fashion store.", "https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=800&q=80", "https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=1600&q=80", 85, createdAt),
                Template("nova-wear", "Nova Wear", "clothing-store", ["Modern", "Casual"], "Modern clothing for a new generation.", "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=800&q=80", "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=1600&q=80", 80, createdAt));
        }

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

    private static Template Template(
        string slug,
        string name,
        string businessType,
        List<string> tags,
        string shortDescription,
        string thumbnailUrl,
        string fullPreviewUrl,
        int popularityScore,
        DateTimeOffset createdAt) => new()
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Name = name,
            BusinessType = businessType.ToLowerInvariant(),
            Tags = tags,
            ShortDescription = shortDescription,
            SearchText = $"{name} {shortDescription} {string.Join(' ', tags)}".ToLowerInvariant(),
            ThumbnailUrl = thumbnailUrl,
            FullPreviewUrl = fullPreviewUrl,
            PopularityScore = popularityScore,
            IsActive = true,
            CreatedAt = createdAt
        };
}
