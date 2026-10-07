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
            .Include(w => w.Themes)
            .ThenInclude(t => t.Pages)
            .ThenInclude(p => p.Elements)
            .FirstOrDefaultAsync(w => w.Id == websiteId, cancellationToken);

        return website is null ? null : ToResponse(website);
    }

    public async Task<IReadOnlyList<WebsiteResponse>> GetWebsitesAsync(CancellationToken cancellationToken)
    {
        var websites = await dbContext.Websites
            .AsNoTracking()
            .Include(w => w.Themes)
            .ThenInclude(t => t.Pages)
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

        var template = await dbContext.Templates.FirstOrDefaultAsync(t => t.TemplateId == request.TemplateId, cancellationToken);
        if (template != null)
        {
            var theme = new Theme
            {
                Id = Guid.NewGuid(),
                WebsiteId = website.Id,
                Name = template.Name,
                IsLive = true,
                Price = 0,
                LastEdited = DateTime.UtcNow
            };
            dbContext.Themes.Add(theme);

            var page = new Page
            {
                Id = Guid.NewGuid(),
                ThemeId = theme.Id,
                Title = "Home",
                Slug = "home",
                IsHomePage = true,
                DisplayOrder = 0
            };
            dbContext.Pages.Add(page);

            // Decode SectionConfiguration if not empty
            bool hasSections = false;
            var sectionConfig = request.SectionConfiguration ?? template.SectionConfiguration;
            if (!string.IsNullOrWhiteSpace(sectionConfig) && sectionConfig != "{}")
            {
                try
                {
                    var sections = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(sectionConfig);
                    if (sections != null && sections.Count > 0)
                    {
                        hasSections = true;
                        int order = 0;
                        foreach (var sec in sections)
                        {
                            var elType = sec.TryGetValue("type", out var typeVal) ? typeVal?.ToString() ?? "text" : "text";
                            dbContext.PageElements.Add(new PageElement
                            {
                                Id = Guid.NewGuid(),
                                PageId = page.Id,
                                ElementType = elType,
                                Name = $"{elType} Section",
                                DisplayOrder = order++,
                                Properties = sec,
                                IsEditable = true
                            });
                        }
                    }
                }
                catch { /* fallback to default */ }
            }

            if (!hasSections)
            {
                dbContext.PageElements.Add(new PageElement
                {
                    Id = Guid.NewGuid(),
                    PageId = page.Id,
                    ElementType = "hero",
                    Name = "Hero",
                    DisplayOrder = 0,
                    Properties = new Dictionary<string, object>
                    {
                        { "title", template.Name },
                        { "subtitle", template.Description ?? "Welcome to your new website!" },
                        { "align", "center" }
                    },
                    IsEditable = true
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Fetch it again to include the newly created themes
        return await GetWebsiteAsync(website.Id, cancellationToken) ?? ToResponse(website);
    }

    public async Task<WebsiteResponse> UpdateWebsiteAsync(Guid websiteId, UpdateWebsiteRequest request, CancellationToken cancellationToken)
    {
        var website = await dbContext.Websites
            .Include(w => w.Themes)
            .ThenInclude(t => t.Pages)
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

        if (!string.IsNullOrWhiteSpace(request.TemplateId) && website.TemplateId != request.TemplateId)
        {
            website.TemplateId = request.TemplateId;
            
            // Delete existing themes to make way for the new template
            dbContext.Themes.RemoveRange(website.Themes);
            
            var template = await dbContext.Templates.FirstOrDefaultAsync(t => t.TemplateId == request.TemplateId, cancellationToken);
            if (template != null)
            {
                var theme = new Theme
                {
                    Id = Guid.NewGuid(),
                    WebsiteId = website.Id,
                    Name = template.Name,
                    IsLive = true,
                    Price = 0,
                    LastEdited = DateTime.UtcNow
                };
                dbContext.Themes.Add(theme);

                var page = new Page
                {
                    Id = Guid.NewGuid(),
                    ThemeId = theme.Id,
                    Title = "Home",
                    Slug = "home",
                    IsHomePage = true,
                    DisplayOrder = 0
                };
                dbContext.Pages.Add(page);

                bool hasSections = false;
                var sectionConfig = request.SectionConfiguration ?? template.SectionConfiguration;
                if (!string.IsNullOrWhiteSpace(sectionConfig) && sectionConfig != "{}")
                {
                    try
                    {
                        var sections = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(sectionConfig);
                        if (sections != null && sections.Count > 0)
                        {
                            hasSections = true;
                            int order = 0;
                            foreach (var sec in sections)
                            {
                                var elType = sec.TryGetValue("type", out var typeVal) ? typeVal?.ToString() ?? "text" : "text";
                                dbContext.PageElements.Add(new PageElement
                                {
                                    Id = Guid.NewGuid(),
                                    PageId = page.Id,
                                    ElementType = elType,
                                    Name = $"{elType} Section",
                                    DisplayOrder = order++,
                                    Properties = sec,
                                    IsEditable = true
                                });
                            }
                        }
                    }
                    catch { /* fallback to default */ }
                }

                if (!hasSections)
                {
                    dbContext.PageElements.Add(new PageElement
                    {
                        Id = Guid.NewGuid(),
                        PageId = page.Id,
                        ElementType = "hero",
                        Name = "Hero",
                        DisplayOrder = 0,
                        Properties = new Dictionary<string, object>
                        {
                            { "title", template.Name },
                            { "subtitle", template.Description ?? "Welcome to your website!" },
                            { "align", "center" }
                        },
                        IsEditable = true
                    });
                }
            }
        }

        website.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        
        // Fetch it again to include the newly created themes
        return await GetWebsiteAsync(website.Id, cancellationToken) ?? ToResponse(website);
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
            website.Themes?.Select(ThemeToResponse).ToList() ?? []);

    private static ThemeResponse ThemeToResponse(Theme theme) =>
        new(
            theme.Id,
            theme.WebsiteId,
            theme.Name,
            theme.IsLive,
            theme.Price,
            theme.ThumbnailUrl,
            theme.LastEdited,
            theme.Pages?.OrderBy(p => p.DisplayOrder).Select(PageToResponse).ToList() ?? []);

    private static PageResponse PageToResponse(Page page) =>
        new(
            page.Id,
            page.ThemeId,
            page.Title,
            page.Slug,
            page.Description,
            page.DisplayOrder,
            page.IsHomePage,
            page.IsHidden,
            page.CreatedAt,
            page.UpdatedAt,
            page.Elements?.OrderBy(e => e.DisplayOrder).Select(ElementToResponse).ToList() ?? []);

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
