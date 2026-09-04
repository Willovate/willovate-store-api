namespace Willovate.Store.Api.Models;

public sealed class PublicationStatus
{
    public Guid Id { get; init; }
    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
