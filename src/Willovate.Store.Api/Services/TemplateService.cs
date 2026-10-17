using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public interface ITemplateService
{
    Task<IReadOnlyList<TemplateResponse>> GetTemplatesAsync(
        string businessType,
        string? tag,
        string? search,
        string? sortBy,
        CancellationToken cancellationToken);

    Task<SelectTemplateResponse> SaveTemplateSelectionAsync(
        SelectTemplateRequest request,
        CancellationToken cancellationToken);
}

public sealed class TemplateService(StoreDbContext dbContext) : ITemplateService
{
    public async Task<IReadOnlyList<TemplateResponse>> GetTemplatesAsync(
        string businessType,
        string? tag,
        string? search,
        string? sortBy,
        CancellationToken cancellationToken)
    {
        var normalizedType = businessType.Trim().ToLowerInvariant();
        var query = dbContext.Templates
            .AsNoTracking()
            .Where(t => t.IsActive && t.BusinessType == normalizedType);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(t => t.SearchText.Contains(term));
        }

        var templates = await query.ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(tag) && !string.Equals(tag, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedTag = tag.Trim();
            templates = templates
                .Where(t => t.Tags.Any(item => string.Equals(item, normalizedTag, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var sorted = sortBy?.ToLowerInvariant() switch
        {
            "newest" => templates.OrderByDescending(t => t.CreatedAt).ToList(),
            "name_asc" => templates.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList(),
            _ => templates.OrderByDescending(t => t.PopularityScore).ToList()
        };

        return sorted.Select(ToResponse).ToList();
    }

    public async Task<SelectTemplateResponse> SaveTemplateSelectionAsync(
        SelectTemplateRequest request,
        CancellationToken cancellationToken)
    {
        string message;

        if (!request.IsBlank && request.TemplateId.HasValue)
        {
            var selectedTemplate = await dbContext.Templates
                .AsNoTracking()
                .Where(t => t.Id == request.TemplateId.Value && t.IsActive)
                .Select(t => t.Name)
                .SingleOrDefaultAsync(cancellationToken);

            if (selectedTemplate is null)
            {
                throw new KeyNotFoundException($"Template with ID '{request.TemplateId.Value}' was not found.");
            }

            message = $"{selectedTemplate} has been saved to your workspace.";
        }
        else
        {
            message = "Your blank workspace has been created.";
        }

        var projectId = $"proj_{Guid.NewGuid():N}"[..12];
        var nextStepUrl = $"/workspace/{projectId}";

        return new SelectTemplateResponse(
            Success: true,
            ProjectId: projectId,
            NextStepUrl: nextStepUrl,
            Message: message);
    }

    private static TemplateResponse ToResponse(Template template) => new(
        template.Id,
        template.Slug,
        template.Name,
        template.BusinessType,
        template.Tags,
        template.ShortDescription,
        template.ThumbnailUrl,
        template.FullPreviewUrl,
        template.PopularityScore,
        template.IsActive);
}
