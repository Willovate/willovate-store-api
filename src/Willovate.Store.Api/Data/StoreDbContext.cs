using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Data;

public sealed class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Website> Websites => Set<Website>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageElement> PageElements => Set<PageElement>();

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

        // Website Configuration
        var website = modelBuilder.Entity<Website>();
        website.HasKey(item => item.Id);
        website.HasIndex(item => item.TemplateId);
        website.Property(item => item.Name).HasMaxLength(200);
        website.Property(item => item.Description).HasMaxLength(1_000);
        website.Property(item => item.TemplateId).HasMaxLength(100);
        website.Property(item => item.ThemeColor).HasMaxLength(20);
        website.HasMany(item => item.Pages).WithOne(item => item.Website).HasForeignKey(item => item.WebsiteId).OnDelete(DeleteBehavior.Cascade);

        // Page Configuration
        var page = modelBuilder.Entity<Page>();
        page.HasKey(item => item.Id);
        page.HasIndex(item => new { item.WebsiteId, item.Slug }).IsUnique();
        page.Property(item => item.Title).HasMaxLength(200);
        page.Property(item => item.Slug).HasMaxLength(120);
        page.Property(item => item.Description).HasMaxLength(1_000);
        page.HasMany(item => item.Elements).WithOne(item => item.Page).HasForeignKey(item => item.PageId).OnDelete(DeleteBehavior.Cascade);

        // PageElement Configuration
        var element = modelBuilder.Entity<PageElement>();
        element.HasKey(item => item.Id);
        element.HasIndex(item => item.PageId);
        element.Property(item => item.ElementType).HasMaxLength(50);
        element.Property(item => item.Name).HasMaxLength(200);
        element.Property(item => item.Properties)
            .HasConversion(
                properties => JsonSerializer.Serialize(properties, (JsonSerializerOptions?)null),
                properties => JsonSerializer.Deserialize<Dictionary<string, object>>(properties, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
            .HasColumnType("jsonb");
    }
}
