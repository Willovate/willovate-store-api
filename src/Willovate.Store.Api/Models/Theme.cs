namespace Willovate.Store.Api.Models;

public class Theme
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Subcategory { get; set; }
    public string? Description { get; set; }
    public string? StyleTags { get; set; }
    public string? TemplateSlug { get; set; }
    public bool IsLive { get; set; }
    public decimal Price { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime LastEdited { get; set; }

    // Navigation properties
    public Website? Website { get; set; }
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}
