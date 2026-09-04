namespace Willovate.Store.Api.Contracts;

public sealed record PublicationStatusResponse(
    bool IsPublished,
    DateTimeOffset? PublishedAt);
