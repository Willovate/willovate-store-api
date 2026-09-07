using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Data;

public sealed class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Template> Templates => Set<Template>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();

        product.HasKey(item => item.Id);
        product.HasIndex(item => item.Slug).IsUnique();
        product.HasIndex(item => item.CategoryKey);
        product.Property(item => item.Slug).HasMaxLength(120);
        product.Property(item => item.Name).HasMaxLength(180);
        product.Property(item => item.Description).HasMaxLength(1_200);
        product.Property(item => item.SearchText).HasMaxLength(1_500);
        product.Property(item => item.Category).HasMaxLength(80);
        product.Property(item => item.CategoryKey).HasMaxLength(80);
        product.Property(item => item.VisualTheme).HasMaxLength(40);
        product.Property(item => item.Price).HasPrecision(18, 2);
        product.Property(item => item.CompareAtPrice).HasPrecision(18, 2);

        var template = modelBuilder.Entity<Template>();

        template.HasKey(item => item.Id);
        template.HasIndex(item => item.Slug).IsUnique();
        template.HasIndex(item => item.BusinessType);
        template.Property(item => item.Slug).HasMaxLength(120);
        template.Property(item => item.Name).HasMaxLength(180);
        template.Property(item => item.BusinessType).HasMaxLength(80);
        template.Property(item => item.ShortDescription).HasMaxLength(500);
        template.Property(item => item.SearchText).HasMaxLength(1_500);
        template.Property(item => item.ThumbnailUrl).HasMaxLength(500);
        template.Property(item => item.FullPreviewUrl).HasMaxLength(500);
    }
}
