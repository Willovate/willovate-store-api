namespace Willovate.Store.Api.Models;

public sealed class Template
{
    public Guid Id { get; init; }
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required string BusinessType { get; set; }
    public required List<string> Tags { get; set; } = [];
    public required string ShortDescription { get; set; }
    public required string SearchText { get; set; }
    public required string ThumbnailUrl { get; set; }
    public required string FullPreviewUrl { get; set; }
    public int PopularityScore { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; }
}

