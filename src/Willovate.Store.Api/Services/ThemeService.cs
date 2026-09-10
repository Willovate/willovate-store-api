using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class ThemeService(StoreDbContext dbContext) : IThemeService
{
    public async Task<ThemeResponse?> GetThemeAsync(Guid themeId, CancellationToken cancellationToken)
    {
        var theme = await dbContext.Themes
            .AsNoTracking()
            .Include(t => t.Pages)
            .ThenInclude(p => p.Elements)
            .FirstOrDefaultAsync(t => t.Id == themeId, cancellationToken);

        return theme is null ? null : ToResponse(theme);
    }

    public async Task<IReadOnlyList<ThemeResponse>> GetThemesByWebsiteAsync(Guid websiteId, CancellationToken cancellationToken)
    {
        var themes = await dbContext.Themes
            .AsNoTracking()
            .Include(t => t.Pages)
            .ThenInclude(p => p.Elements)
            .Where(t => t.WebsiteId == websiteId)
            .OrderByDescending(t => t.LastEdited)
            .ToListAsync(cancellationToken);

        return themes.ConvertAll(ToResponse);
    }

    public async Task<ThemeResponse> CreateThemeAsync(Guid websiteId, CreateThemeRequest request, CancellationToken cancellationToken)
    {
        var websiteExists = await dbContext.Websites.AnyAsync(w => w.Id == websiteId, cancellationToken);
        if (!websiteExists)
            throw new KeyNotFoundException($"Website {websiteId} not found");

        var theme = new Theme
        {
            Id = Guid.NewGuid(),
            WebsiteId = websiteId,
            Name = request.Name,
            IsLive = false,
            LastEdited = DateTime.UtcNow
        };

        dbContext.Themes.Add(theme);

        if (request.DuplicateFromLive)
        {
            var liveTheme = await dbContext.Themes
                .AsNoTracking()
                .Include(t => t.Pages)
                .ThenInclude(p => p.Elements)
                .FirstOrDefaultAsync(t => t.WebsiteId == websiteId && t.IsLive, cancellationToken);

            if (liveTheme is null)
            {
                // Just create empty if no live theme
                var homePageId = Guid.NewGuid();
                dbContext.Pages.Add(new Page { Id = homePageId, ThemeId = theme.Id, Title = "Home", Slug = "home", IsHomePage = true });
            }
            else
            {
                foreach (var page in liveTheme.Pages)
                {
                    var newPageId = Guid.NewGuid();
                    var newPage = new Page
                    {
                        Id = newPageId,
                        ThemeId = theme.Id,
                        Title = page.Title,
                        Slug = page.Slug,
                        Description = page.Description,
                        DisplayOrder = page.DisplayOrder,
                        IsHomePage = page.IsHomePage,
                        IsHidden = page.IsHidden
                    };
                    dbContext.Pages.Add(newPage);

                    foreach (var element in page.Elements)
                    {
                        var newElement = new PageElement
                        {
                            Id = Guid.NewGuid(),
                            PageId = newPageId,
                            ElementType = element.ElementType,
                            Name = element.Name,
                            DisplayOrder = element.DisplayOrder,
                            Properties = new Dictionary<string, object>(element.Properties ?? new()),
                            IsEditable = element.IsEditable,
                            IsRequired = element.IsRequired
                        };
                        dbContext.PageElements.Add(newElement);
                    }
                }
            }
        }
        else
        {
            // Empty theme
            var homePageId = Guid.NewGuid();
            dbContext.Pages.Add(new Page { Id = homePageId, ThemeId = theme.Id, Title = "Home", Slug = "home", IsHomePage = true });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        
        return await GetThemeAsync(theme.Id, cancellationToken) ?? ToResponse(theme);
    }

    public async Task<ThemeResponse> UpdateThemeAsync(Guid themeId, UpdateThemeRequest request, CancellationToken cancellationToken)
    {
        var theme = await dbContext.Themes
            .Include(t => t.Pages)
            .ThenInclude(p => p.Elements)
            .FirstOrDefaultAsync(t => t.Id == themeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Theme {themeId} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            theme.Name = request.Name;

        if (request.IsLive.HasValue)
            theme.IsLive = request.IsLive.Value;

        theme.LastEdited = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(theme);
    }

    public async Task DeleteThemeAsync(Guid themeId, CancellationToken cancellationToken)
    {
        var theme = await dbContext.Themes.FindAsync([themeId], cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Theme {themeId} not found");

        if (theme.IsLive)
        {
            throw new InvalidOperationException("Cannot delete the live theme.");
        }

        dbContext.Themes.Remove(theme);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ThemeResponse> PublishThemeAsync(Guid themeId, CancellationToken cancellationToken)
    {
        var theme = await dbContext.Themes
            .Include(t => t.Pages)
            .ThenInclude(p => p.Elements)
            .FirstOrDefaultAsync(t => t.Id == themeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Theme {themeId} not found");

        var websiteThemes = await dbContext.Themes
            .Where(t => t.WebsiteId == theme.WebsiteId)
            .ToListAsync(cancellationToken);

        foreach (var t in websiteThemes)
        {
            t.IsLive = (t.Id == themeId);
        }

        theme.LastEdited = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(theme);
    }

    public async Task<ThemeResponse> DuplicateThemeAsync(Guid themeId, CancellationToken cancellationToken)
    {
        var theme = await dbContext.Themes
            .Include(t => t.Pages)
            .ThenInclude(p => p.Elements)
            .FirstOrDefaultAsync(t => t.Id == themeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Theme {themeId} not found");

        return await CreateThemeAsync(theme.WebsiteId, new CreateThemeRequest($"{theme.Name} (Copy)", true), cancellationToken);
    }

    private static ThemeResponse ToResponse(Theme theme) =>
        new(
            theme.Id,
            theme.WebsiteId,
            theme.Name,
            theme.IsLive,
            theme.LastEdited,
            theme.Pages?.OrderBy(p => p.DisplayOrder).Select(PageToResponse).ToList() ?? new List<PageResponse>());

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
            page.Elements?.OrderBy(e => e.DisplayOrder).Select(ElementToResponse).ToList() ?? new List<PageElementResponse>());

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
