using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Data;

public sealed class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();

        product.HasKey(item => item.Id);
        product.HasIndex(item => item.Slug).IsUnique();
        product.HasIndex(item => item.CategoryKey);
        product.HasIndex(item => item.IsActive);
        product.Property(item => item.Slug).HasMaxLength(120);
        product.Property(item => item.Name).HasMaxLength(180);
        product.Property(item => item.Description).HasMaxLength(50_000);
        product.Property(item => item.SearchText).HasMaxLength(5_000);
        product.Property(item => item.Category).HasMaxLength(300);
        product.Property(item => item.CategoryKey).HasMaxLength(300);
        product.Property(item => item.VisualTheme).HasMaxLength(40);
        product.Property(item => item.Price).HasPrecision(18, 2);
        product.Property(item => item.CompareAtPrice).HasPrecision(18, 2);

        // Admin fields
        product.Property(item => item.IsActive).HasDefaultValue(true);
        product.Property(item => item.Sku).HasMaxLength(120);
        product.Property(item => item.ProductType).HasMaxLength(80);
        product.Property(item => item.Tags).HasMaxLength(500);
        product.Property(item => item.ImageUrls).HasMaxLength(2_000).HasDefaultValue("[]");
        product.Property(item => item.Variants).HasMaxLength(100_000);
        product.Property(item => item.UpdatedAt);
    }
}
