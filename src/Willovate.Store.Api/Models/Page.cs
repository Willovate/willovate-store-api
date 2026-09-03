namespace Willovate.Store.Api.Models;

public sealed class Page
{
    public Guid Id { get; init; }
    public Guid WebsiteId { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsHomePage { get; set; }
    public bool IsHidden { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public Website? Website { get; set; }
    public ICollection<PageElement> Elements { get; } = [];
}
