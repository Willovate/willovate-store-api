using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Data;

public static class TemplateSeeder
{
    public static async Task SeedAsync(StoreDbContext dbContext)
    {
        if (await dbContext.Templates.AnyAsync()) return;

        var templates = new List<Template>
        {

            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "noir-ember",
                Name = "Noir Ember",
                Category = "fine-dining",
                Description = "Noir Ember template for fine-dining",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "ivory-court",
                Name = "Ivory Court",
                Category = "fine-dining",
                Description = "Ivory Court template for fine-dining",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 2,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "azure-bistro",
                Name = "Azure Bistro",
                Category = "fine-dining",
                Description = "Azure Bistro template for fine-dining",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 3,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "velvet-table",
                Name = "Velvet Table",
                Category = "fine-dining",
                Description = "Velvet Table template for fine-dining",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 4,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "minimal-omakase",
                Name = "Minimal Omakase",
                Category = "fine-dining",
                Description = "Minimal Omakase template for fine-dining",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 5,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "morning-ritual",
                Name = "Morning Ritual",
                Category = "cafe",
                Description = "Morning Ritual template for cafe",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 6,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "brew-house",
                Name = "Brew House",
                Category = "cafe",
                Description = "Brew House template for cafe",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 7,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "corner-cafe",
                Name = "Corner Cafe",
                Category = "cafe",
                Description = "Corner Cafe template for cafe",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 8,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "latte-lane",
                Name = "Latte Lane",
                Category = "cafe",
                Description = "Latte Lane template for cafe",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 9,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "the-daily-grind",
                Name = "The Daily Grind",
                Category = "cafe",
                Description = "The Daily Grind template for cafe",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 10,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "golden-crumb",
                Name = "Golden Crumb",
                Category = "bakery",
                Description = "Golden Crumb template for bakery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 11,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "rustic-oven",
                Name = "Rustic Oven",
                Category = "bakery",
                Description = "Rustic Oven template for bakery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 12,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "patisserie-lune",
                Name = "Patisserie Lune",
                Category = "bakery",
                Description = "Patisserie Lune template for bakery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 13,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "rise-and-knead",
                Name = "Rise & Knead",
                Category = "bakery",
                Description = "Rise & Knead template for bakery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 14,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "sugar-petal",
                Name = "Sugar Petal",
                Category = "bakery",
                Description = "Sugar Petal template for bakery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 15,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "street-bites",
                Name = "Street Bites",
                Category = "fast-food",
                Description = "Street Bites template for fast-food",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 16,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "crunch-box",
                Name = "Crunch Box",
                Category = "fast-food",
                Description = "Crunch Box template for fast-food",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 17,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "burger-blitz",
                Name = "Burger Blitz",
                Category = "fast-food",
                Description = "Burger Blitz template for fast-food",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 18,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "wrap-rush",
                Name = "Wrap Rush",
                Category = "fast-food",
                Description = "Wrap Rush template for fast-food",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 19,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "quick-bowl",
                Name = "Quick Bowl",
                Category = "fast-food",
                Description = "Quick Bowl template for fast-food",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 20,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "dark-kitchen-pro",
                Name = "Dark Kitchen Pro",
                Category = "cloud-kitchen",
                Description = "Dark Kitchen Pro template for cloud-kitchen",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 21,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "ghost-chef",
                Name = "Ghost Chef",
                Category = "cloud-kitchen",
                Description = "Ghost Chef template for cloud-kitchen",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 22,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "fresh-batch",
                Name = "Fresh Batch",
                Category = "cloud-kitchen",
                Description = "Fresh Batch template for cloud-kitchen",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 23,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "flame-hub",
                Name = "Flame Hub",
                Category = "cloud-kitchen",
                Description = "Flame Hub template for cloud-kitchen",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 24,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "box-and-go",
                Name = "Box And Go",
                Category = "cloud-kitchen",
                Description = "Box And Go template for cloud-kitchen",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 25,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "napoli-fire",
                Name = "Napoli Fire",
                Category = "pizza",
                Description = "Napoli Fire template for pizza",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 26,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "pie-lab",
                Name = "Pie Lab",
                Category = "pizza",
                Description = "Pie Lab template for pizza",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 27,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "crust-theory",
                Name = "Crust Theory",
                Category = "pizza",
                Description = "Crust Theory template for pizza",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 28,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "mammas-table",
                Name = "Mammas Table",
                Category = "pizza",
                Description = "Mammas Table template for pizza",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 29,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "slice-society",
                Name = "Slice Society",
                Category = "pizza",
                Description = "Slice Society template for pizza",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 30,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "masala-royale",
                Name = "Masala Royale",
                Category = "indian",
                Description = "Masala Royale template for indian",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 31,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "spice-route",
                Name = "Spice Route",
                Category = "indian",
                Description = "Spice Route template for indian",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 32,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "chai-and-chaat",
                Name = "Chai And Chaat",
                Category = "indian",
                Description = "Chai And Chaat template for indian",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 33,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "tandoor-nights",
                Name = "Tandoor Nights",
                Category = "indian",
                Description = "Tandoor Nights template for indian",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 34,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "tiffin-tales",
                Name = "Tiffin Tales",
                Category = "indian",
                Description = "Tiffin Tales template for indian",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 35,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "frost-and-fruit",
                Name = "Frost And Fruit",
                Category = "dessert-shop",
                Description = "Frost And Fruit template for dessert-shop",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 36,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "sweet-tooth",
                Name = "Sweet Tooth",
                Category = "dessert-shop",
                Description = "Sweet Tooth template for dessert-shop",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 37,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "choco-vault",
                Name = "Choco Vault",
                Category = "dessert-shop",
                Description = "Choco Vault template for dessert-shop",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 38,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "scoop-story",
                Name = "Scoop Story",
                Category = "dessert-shop",
                Description = "Scoop Story template for dessert-shop",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 39,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "waffle-house-studio",
                Name = "Waffle House Studio",
                Category = "dessert-shop",
                Description = "Waffle House Studio template for dessert-shop",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 40,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "fork-express",
                Name = "Fork Express",
                Category = "food-delivery",
                Description = "Fork Express template for food-delivery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 41,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "dash-eats",
                Name = "Dash Eats",
                Category = "food-delivery",
                Description = "Dash Eats template for food-delivery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 42,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "local-plate",
                Name = "Local Plate",
                Category = "food-delivery",
                Description = "Local Plate template for food-delivery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 43,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "hungry-hero",
                Name = "Hungry Hero",
                Category = "food-delivery",
                Description = "Hungry Hero template for food-delivery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 44,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "zip-meals",
                Name = "Zip Meals",
                Category = "food-delivery",
                Description = "Zip Meals template for food-delivery",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 45,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "smokehouse-77",
                Name = "Smokehouse 77",
                Category = "bbq-grill",
                Description = "Smokehouse 77 template for bbq-grill",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 46,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "grill-republic",
                Name = "Grill Republic",
                Category = "bbq-grill",
                Description = "Grill Republic template for bbq-grill",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 47,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "fire-and-rib",
                Name = "Fire And Rib",
                Category = "bbq-grill",
                Description = "Fire And Rib template for bbq-grill",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 48,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "backyard-barbeque",
                Name = "Backyard Barbeque",
                Category = "bbq-grill",
                Description = "Backyard Barbeque template for bbq-grill",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 49,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
            new Template
            {
                Id = Guid.NewGuid(),
                TemplateId = "kebab-kingdom",
                Name = "Kebab Kingdom",
                Category = "bbq-grill",
                Description = "Kebab Kingdom template for bbq-grill",
                Status = "Active",
                IsAvailable = true,
                DisplayOrder = 50,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ThemeConfiguration = "{}",
                SectionConfiguration = "{}",
                ImageConfiguration = "{}"
            },
        };

        dbContext.Templates.AddRange(templates);
        await dbContext.SaveChangesAsync();
    }
}
