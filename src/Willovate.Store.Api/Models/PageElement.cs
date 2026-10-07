namespace Willovate.Store.Api.Models;

public sealed class PageElement
{
    public Guid Id { get; init; }
    public Guid PageId { get; set; }
    public required string ElementType { get; set; } // "text", "image", "button", "section", etc.
    public required string Name { get; set; }
    public int DisplayOrder { get; set; }
    public Dictionary<string, object>? Properties { get; set; } // Content, styling, etc.
    public bool IsEditable { get; set; } = true;
    public bool IsRequired { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public Page? Page { get; set; }
}
