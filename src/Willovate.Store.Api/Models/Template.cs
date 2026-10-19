namespace Willovate.Store.Api.Models;

public sealed class Template
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string TemplateId { get; set; } = string.Empty; // e.g., 'smokehouse-77'
    public string Slug { get; set; } = string.Empty;
    public required string Name { get; set; }
    public string Category { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string? Description { get; set; }
    public string ShortDescription { get; set; } = string.Empty;
    public string SearchText { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string? Status { get; set; }
    public string? Badge { get; set; }
    public string? PreviewImage { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string FullPreviewUrl { get; set; } = string.Empty;
    public string? ThemeConfiguration { get; set; } // JSON
    public string? SectionConfiguration { get; set; } // JSON
    public string? ImageConfiguration { get; set; } // JSON
    public int PopularityScore { get; set; } = 100;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
