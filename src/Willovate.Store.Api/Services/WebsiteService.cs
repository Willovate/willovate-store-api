using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class WebsiteService(StoreDbContext dbContext) : IWebsiteService
{
    public async Task<WebsiteResponse?> GetWebsiteAsync(Guid websiteId, CancellationToken cancellationToken)
    {
        var website = await dbContext.Websites
            .AsNoTracking()
            .Include(w => w.Pages)
            .ThenInclude(p => p.Elements)
            .FirstOrDefaultAsync(w => w.Id == websiteId, cancellationToken);

        return website is null ? null : ToResponse(website);
    }

    public async Task<IReadOnlyList<WebsiteResponse>> GetWebsitesAsync(CancellationToken cancellationToken)
    {
        var websites = await dbContext.Websites
            .AsNoTracking()
            .Include(w => w.Pages)
            .ThenInclude(p => p.Elements)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return websites.ConvertAll(ToResponse);
    }

    public async Task<WebsiteResponse> CreateWebsiteAsync(CreateWebsiteRequest request, CancellationToken cancellationToken)
    {
        var website = new Website
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            TemplateId = request.TemplateId,
            ThemeColor = request.ThemeColor,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsPublished = false
        };

        dbContext.Websites.Add(website);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(website);
    }

    public async Task<WebsiteResponse> UpdateWebsiteAsync(Guid websiteId, UpdateWebsiteRequest request, CancellationToken cancellationToken)
    {
        var website = await dbContext.Websites
            .Include(w => w.Pages)
            .ThenInclude(p => p.Elements)
            .FirstOrDefaultAsync(w => w.Id == websiteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Website {websiteId} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            website.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Description))
            website.Description = request.Description;

        if (request.ThemeColor is not null)
            website.ThemeColor = request.ThemeColor;

        if (request.IsPublished.HasValue)
            website.IsPublished = request.IsPublished.Value;

        website.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(website);
    }

    public async Task DeleteWebsiteAsync(Guid websiteId, CancellationToken cancellationToken)
    {
        var website = await dbContext.Websites.FindAsync([websiteId], cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Website {websiteId} not found");

        dbContext.Websites.Remove(website);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static WebsiteResponse ToResponse(Website website) =>
        new(
            website.Id,
            website.Name,
            website.Description,
            website.TemplateId,
            website.ThemeColor,
            website.IsPublished,
            website.CreatedAt,
            website.UpdatedAt,
            website.Pages.OrderBy(p => p.DisplayOrder).Select(PageToResponse).ToList());

    private static PageResponse PageToResponse(Page page) =>
        new(
            page.Id,
            page.WebsiteId,
            page.Title,
            page.Slug,
            page.Description,
            page.DisplayOrder,
            page.IsHomePage,
            page.IsHidden,
            page.CreatedAt,
            page.UpdatedAt,
            page.Elements.OrderBy(e => e.DisplayOrder).Select(ElementToResponse).ToList());

    private static PageElementResponse ElementToResponse(PageElement element) =>
        new(
            element.Id,
            element.PageId,
            element.ElementType,
            element.Name,
            element.DisplayOrder,
            element.Properties,
            element.IsEditable,
            element.IsRequired,
            element.CreatedAt,
            element.UpdatedAt);
}
