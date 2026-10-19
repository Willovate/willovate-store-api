namespace Willovate.Store.Api.Contracts;

public sealed record TemplateResponse(
    Guid Id,
    string Slug,
    string Name,
    string BusinessType,
    IReadOnlyList<string> Tags,
    string ShortDescription,
    string ThumbnailUrl,
    string FullPreviewUrl,
    int PopularityScore,
    bool IsActive);

public sealed record SelectTemplateRequest(
    string SessionId,
    Guid? TemplateId,
    bool IsBlank);

public sealed record SelectTemplateResponse(
    bool Success,
    string ProjectId,
    string NextStepUrl,
    string Message);

