using Microsoft.EntityFrameworkCore;
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
}

public class TemplateService(StoreDbContext dbContext) : ITemplateService
{
    public async Task<IReadOnlyList<Template>> GetAllTemplatesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Templates
            .Where(t => t.IsAvailable)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Template>> GetTemplatesByCategoryAsync(string categoryId, CancellationToken cancellationToken)
    {
        return await dbContext.Templates
            .Where(t => t.IsAvailable && t.Category == categoryId)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Template>> SearchTemplatesAsync(string query, CancellationToken cancellationToken)
    {
        return await dbContext.Templates
            .Where(t => t.IsAvailable && (t.Name.Contains(query) || (t.Description != null && t.Description.Contains(query))))
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Template?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Templates.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Template?> GetTemplateByTemplateIdAsync(string templateId, CancellationToken cancellationToken)
    {
        return await dbContext.Templates.FirstOrDefaultAsync(t => t.TemplateId == templateId, cancellationToken);
    }

    public async Task<Template> CreateTemplateAsync(Template templateModel, CancellationToken cancellationToken)
    {
        dbContext.Templates.Add(templateModel);
        await dbContext.SaveChangesAsync(cancellationToken);
        return templateModel;
    }
}
