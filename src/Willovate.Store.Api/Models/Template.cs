namespace Willovate.Store.Api.Models;

public sealed class Template
{
    public Guid Id { get; init; }
    public required string TemplateId { get; set; } // e.g., 'smokehouse-77'
    public required string Name { get; set; }
    public required string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Badge { get; set; }
    public string? PreviewImage { get; set; }
    public string? ThemeConfiguration { get; set; } // JSON
    public string? SectionConfiguration { get; set; } // JSON
    public string? ImageConfiguration { get; set; } // JSON
    public int DisplayOrder { get; set; }
    public bool IsAvailable { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
