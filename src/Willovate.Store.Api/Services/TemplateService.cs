using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public interface ITemplateService
{
    Task<IReadOnlyList<Template>> GetAllTemplatesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Template>> GetTemplatesByCategoryAsync(string categoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Template>> SearchTemplatesAsync(string query, CancellationToken cancellationToken);
    Task<Template?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Template?> GetTemplateByTemplateIdAsync(string templateId, CancellationToken cancellationToken);
    Task<Template> CreateTemplateAsync(Template templateModel, CancellationToken cancellationToken);

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
    public async Task<IReadOnlyList<Template>> GetAllTemplatesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Templates
            .Where(t => t.IsAvailable && t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Template>> GetTemplatesByCategoryAsync(string categoryId, CancellationToken cancellationToken)
    {
        var norm = categoryId.Trim().ToLowerInvariant();
        return await dbContext.Templates
            .Where(t => t.IsAvailable && t.IsActive && (t.Category == norm || t.BusinessType == norm))
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Template>> SearchTemplatesAsync(string query, CancellationToken cancellationToken)
    {
        var norm = query.Trim().ToLowerInvariant();
        return await dbContext.Templates
            .Where(t => t.IsAvailable && t.IsActive && (t.Name.Contains(norm) || (t.Description != null && t.Description.Contains(norm)) || t.SearchText.Contains(norm)))
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Template?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Templates.FindAsync([id], cancellationToken);
    }

    public async Task<Template?> GetTemplateByTemplateIdAsync(string templateId, CancellationToken cancellationToken)
    {
        return await dbContext.Templates.FirstOrDefaultAsync(t => t.TemplateId == templateId || t.Slug == templateId, cancellationToken);
    }

    public async Task<Template> CreateTemplateAsync(Template templateModel, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(templateModel.Slug)) templateModel.Slug = templateModel.TemplateId;
        if (string.IsNullOrEmpty(templateModel.BusinessType)) templateModel.BusinessType = templateModel.Category;
        dbContext.Templates.Add(templateModel);
        await dbContext.SaveChangesAsync(cancellationToken);
        return templateModel;
    }

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
            .Where(t => t.IsActive && (t.BusinessType == normalizedType || t.Category == normalizedType));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(t => t.SearchText.Contains(term) || t.Name.Contains(term) || (t.Description != null && t.Description.Contains(term)));
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
            _ => templates.OrderByDescending(t => t.PopularityScore).ThenBy(t => t.DisplayOrder).ToList()
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
        !string.IsNullOrEmpty(template.Slug) ? template.Slug : template.TemplateId,
        template.Name,
        !string.IsNullOrEmpty(template.BusinessType) ? template.BusinessType : template.Category,
        template.Tags ?? [],
        !string.IsNullOrEmpty(template.ShortDescription) ? template.ShortDescription : (template.Description ?? string.Empty),
        !string.IsNullOrEmpty(template.ThumbnailUrl) ? template.ThumbnailUrl : (template.PreviewImage ?? string.Empty),
        !string.IsNullOrEmpty(template.FullPreviewUrl) ? template.FullPreviewUrl : (template.PreviewImage ?? string.Empty),
        template.PopularityScore,
        template.IsActive && template.IsAvailable);
}
