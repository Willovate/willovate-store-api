namespace Willovate.Store.Api.Models;

public sealed class Website
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string TemplateId { get; set; }
    public string? ThemeColor { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsPublished { get; set; }

    // Navigation
    public ICollection<Page> Pages { get; } = [];
}
