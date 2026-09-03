namespace Willovate.Store.Api.Contracts;

public sealed record WebsiteResponse(
    Guid Id,
    string Name,
    string Description,
    string TemplateId,
    string? ThemeColor,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<PageResponse> Pages);

public sealed record PageResponse(
    Guid Id,
    Guid WebsiteId,
    string Title,
    string Slug,
    string? Description,
    int DisplayOrder,
    bool IsHomePage,
    bool IsHidden,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<PageElementResponse> Elements);

public sealed record PageElementResponse(
    Guid Id,
    Guid PageId,
    string ElementType,
    string Name,
    int DisplayOrder,
    Dictionary<string, object>? Properties,
    bool IsEditable,
    bool IsRequired,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

// Request DTOs
public sealed record CreateWebsiteRequest(
    string Name,
    string Description,
    string TemplateId,
    string? ThemeColor = null);

public sealed record UpdateWebsiteRequest(
    string? Name = null,
    string? Description = null,
    string? ThemeColor = null,
    bool? IsPublished = null);

public sealed record CreatePageRequest(
    string Title,
    string Slug,
    string? Description = null,
    int DisplayOrder = 0,
    bool IsHomePage = false,
    bool IsHidden = false);

public sealed record UpdatePageRequest(
    string? Title = null,
    string? Slug = null,
    string? Description = null,
    int? DisplayOrder = null,
    bool? IsHomePage = null,
    bool? IsHidden = null);

public sealed record CreatePageElementRequest(
    string ElementType,
    string Name,
    int DisplayOrder = 0,
    Dictionary<string, object>? Properties = null,
    bool IsEditable = true);

public sealed record UpdatePageElementRequest(
    string? Name = null,
    int? DisplayOrder = null,
    Dictionary<string, object>? Properties = null,
    bool? IsEditable = null);
