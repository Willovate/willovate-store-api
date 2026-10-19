using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Data;

public static class SeedData
{
    /// <summary>
    /// Well-known ID used by the storefront UI to link directly into the workspace.
    /// </summary>
    public static readonly Guid DefaultWebsiteId = new("a1b2c3d4-0000-0000-0000-000000000001");

    public static async Task InitialiseAsync(StoreDbContext dbContext)
    {
        var createdAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                // Café Collection
                Product("artisan-espresso", "Signature House Double Espresso", "Full-bodied double extraction with notes of dark cocoa, toasted hazelnut, and golden crema.", "Coffee & Espresso", 3.75m, 4.25m, 50, "coffee", true, createdAt),
                Product("velvet-oat-flat-white", "Velvet Oat Flat White", "Double shot specialty espresso with silky micro-foamed organic oat milk.", "Coffee & Espresso", 4.85m, null, 40, "coffee", true, createdAt),
                Product("nitro-cold-brew", "High-Altitude Nitro Cold Brew", "Steeped cold for 20 hours and nitrogen-charged for a creamy, cascading stout-like head.", "Coffee & Espresso", 5.25m, 5.75m, 35, "coffee", true, createdAt),
                Product("ceremonial-matcha-latte", "Kyoto Ceremonial Matcha Latte", "First-harvest stone-ground Uji matcha whisked with velvety steamed almond milk and touch of agave.", "Tea & Matcha", 5.50m, null, 30, "tea", true, createdAt),
                Product("spiced-cardamom-latte", "Spiced Cardamom & Honey Latte", "Fresh ground green cardamom infused with raw clover honey, double espresso, and steamed whole milk.", "Coffee & Espresso", 5.40m, null, 25, "coffee", false, createdAt),
                Product("ethiopian-yirgacheffe-beans", "Ethiopian Yirgacheffe Whole Bean (250g)", "Single origin washed process. Jasmine floral fragrance with sweet bergamot and white peach finish.", "Coffee Beans", 18.50m, 21.00m, 20, "coffee", true, createdAt),
                Product("guatemalan-antigua-beans", "Guatemalan Antigua Dark Roast (250g)", "Volcanic terroir. Rich dark chocolate, caramelized raw sugar, and smoky cedar wood notes.", "Coffee Beans", 17.00m, null, 25, "coffee", false, createdAt),
                Product("smoked-turkey-havarti-croissant", "Smoked Turkey & Havarti Croissant", "All-butter croissant filled with peppered smoked turkey, aged havarti, and whole-grain Dijon.", "Brunch & Savory", 8.95m, null, 20, "food", true, createdAt),
                Product("avocado-sourdough-tartine", "Avocado Sourdough Tartine", "Toasted heirloom sourdough, smashed Hass avocado, radish pearls, Egyptian dukkah, and cold-pressed olive oil.", "Brunch & Savory", 9.50m, null, 15, "food", true, createdAt),

                // Bakery Collection
                Product("heritage-country-sourdough", "48-Hour Heritage Country Sourdough", "Naturally leavened with our 40-year wild starter. Open glossy crumb with blistered, caramelized crust.", "Artisan Breads", 8.50m, 9.50m, 30, "bread", true, createdAt),
                Product("seeded-multigrain-batard", "Seeded Multigrain Batard", "Ancient grain flour blend packed with toasted pumpkin, golden flax, and toasted sunflower seeds.", "Artisan Breads", 9.00m, null, 25, "bread", false, createdAt),
                Product("olive-rosemary-fougasse", "Kalamata Olive & Rosemary Fougasse", "French hearth flatbread studded with whole Kalamata olives, garden rosemary, and sea salt flakes.", "Artisan Breads", 7.50m, null, 20, "bread", false, createdAt),
                Product("traditional-french-baguette", "Classic Paris Tradition Baguette", "Crisp shattering crust with an airy honeycomb crumb. Baked fresh three times each morning.", "Artisan Breads", 4.25m, null, 40, "bread", true, createdAt),
                Product("signature-butter-croissant", "Isigny Butter Viennoiserie Croissant", "Crafted with 84% Normandy churned butter. 27 delicate laminations for featherlight flakiness.", "Pastries", 4.50m, null, 50, "pastry", true, createdAt),
                Product("valrhona-pain-au-chocolat", "Double Valrhona Pain au Chocolat", "Two batons of 64% Valrhona dark chocolate wrapped inside golden caramelized puff pastry.", "Pastries", 5.25m, null, 45, "pastry", true, createdAt),
                Product("wild-blueberry-frangipane-tart", "Wild Blueberry Frangipane Tart", "Almond frangipane cream baked with wild mountain blueberries in a sweet butter sablée crust.", "Pastries", 6.50m, null, 25, "pastry", false, createdAt),
                Product("parisian-macaron-gift-box", "Parisian Macaron Luxury Gift Box (12 Pcs)", "Pistachio, Raspberry Rose, Salted Caramel, Dark Chocolate, Passionfruit, and Bourbon Vanilla.", "Gift Boxes", 28.00m, 32.00m, 15, "gifts", true, createdAt),
                Product("tahitian-vanilla-celebration-cake", "Tahitian Vanilla & Salted Caramel Cake (6\")", "Three tiers of golden sponge, house salted caramel, and Swiss meringue buttercream. Serves 8-10.", "Celebration Cakes", 48.00m, 55.00m, 10, "cake", true, createdAt),
                Product("ruby-mirror-entremet", "Ruby Rose & Lychee Mirror Glaze Entremet", "Rose sponge, lychee compote, and raspberry Bavarian cream finished in high-gloss mirror glaze.", "Celebration Cakes", 42.00m, null, 8, "cake", true, createdAt),
                Product("spiced-carrot-pecan-cake", "Spiced Carrot & Roasted Pecan Cake", "Moist heirloom carrot cake with toasted candied pecans and brown butter cream cheese frosting.", "Celebration Cakes", 44.00m, null, 12, "cake", false, createdAt),
                Product("cinnamon-brioche-morning-buns", "Cardamom & Brown Sugar Brioche Morning Bun", "Laminated brioche rolled with Vietnamese cinnamon, crushed green cardamom, and raw demerara sugar.", "Pastries", 5.00m, null, 35, "pastry", true, createdAt),

                // General Goods
                Product("cloud-linen-shirt", "Cloud Linen Apron & Shirt", "Relaxed tailoring in breathable European natural linen.", "Apparel", 2499, 3199, 18, "sky", false, createdAt),
                Product("orbit-desk-lamp", "Orbit Ceramic Cafe Lamp", "Warm, focused light with a sculptural matte terracotta finish.", "Home", 3899, null, 9, "sun", false, createdAt),
                Product("daybreak-tote", "Daybreak Canvas Bakery Tote", "Spacious heavy-duty tote bag made from 100% recycled canvas.", "Accessories", 1799, 2199, 24, "coral", false, createdAt),
                Product("stillness-candle", "Hearth & Coffee Soy Candle", "Cedar, espresso, vanilla bean and rain with a clean burning cotton wick.", "Home", 899, null, 34, "lavender", false, createdAt));
        }

        if (!await dbContext.Websites.AnyAsync(w => w.Id == DefaultWebsiteId))
        {
            var website = new Website
            {
                Id = DefaultWebsiteId,
                Name = "My Willovate Store",
                Description = "A curated store of thoughtful everyday goods.",
                TemplateId = "luxe",
                ThemeColor = "#53674a",
                IsPublished = false,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            };

            dbContext.Websites.Add(website);
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
}
