using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

#pragma warning disable CA1862

namespace Willovate.Store.Api.Services;

public sealed class PageService(StoreDbContext dbContext) : IPageService
{
    public async Task<PageResponse?> GetPageAsync(Guid pageId, CancellationToken cancellationToken)
    {
        var page = await dbContext.Pages
            .AsNoTracking()
            .Include(p => p.Elements)
            .FirstOrDefaultAsync(p => p.Id == pageId, cancellationToken);

        return page is null ? null : ToResponse(page);
    }

    public async Task<IReadOnlyList<PageResponse>> GetPagesByThemeAsync(Guid themeId, CancellationToken cancellationToken)
    {
        var pages = await dbContext.Pages
            .AsNoTracking()
            .Include(p => p.Elements)
            .Where(p => p.ThemeId == themeId)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        return pages.ConvertAll(ToResponse);
    }

    public async Task<PageResponse> CreatePageAsync(Guid themeId, CreatePageRequest request, CancellationToken cancellationToken)
    {
        // Verify theme exists
        var themeExists = await dbContext.Themes.AnyAsync(t => t.Id == themeId, cancellationToken);
        if (!themeExists)
            throw new KeyNotFoundException($"Theme {themeId} not found");

        // Check for duplicate slug within the theme
        var slugExists = await dbContext.Pages.AnyAsync(
            p => p.ThemeId == themeId && p.Slug == request.Slug.ToLowerInvariant(),
            cancellationToken);
        if (slugExists)
            throw new InvalidOperationException($"A page with slug '{request.Slug}' already exists in this website");

        var page = new Page
        {
            Id = Guid.NewGuid(),
            ThemeId = themeId,
            Title = request.Title,
            Slug = request.Slug.ToLowerInvariant(),
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsHomePage = request.IsHomePage,
            IsHidden = request.IsHidden,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Pages.Add(page);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(page);
    }

    public async Task<PageResponse> UpdatePageAsync(Guid pageId, UpdatePageRequest request, CancellationToken cancellationToken)
    {
        var page = await dbContext.Pages
            .Include(p => p.Elements)
            .FirstOrDefaultAsync(p => p.Id == pageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        if (!string.IsNullOrWhiteSpace(request.Title))
            page.Title = request.Title;

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var newSlug = request.Slug.ToLowerInvariant();
            var slugExists = await dbContext.Pages.AnyAsync(
                p => p.ThemeId == page.ThemeId && p.Slug == newSlug && p.Id != pageId,
                cancellationToken);
            if (slugExists)
                throw new InvalidOperationException($"A page with slug '{newSlug}' already exists in this theme");
            page.Slug = newSlug;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
            page.Description = request.Description;

        if (request.DisplayOrder.HasValue)
            page.DisplayOrder = request.DisplayOrder.Value;

        if (request.IsHomePage.HasValue)
            page.IsHomePage = request.IsHomePage.Value;

        if (request.IsHidden.HasValue)
            page.IsHidden = request.IsHidden.Value;

        page.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(page);
    }

    public async Task DeletePageAsync(Guid pageId, CancellationToken cancellationToken)
    {
        var page = await dbContext.Pages.FindAsync([pageId], cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        dbContext.Pages.Remove(page);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PageResponse> GetPageBySlugAsync(Guid themeId, string slug, CancellationToken cancellationToken)
    {
        var page = await dbContext.Pages
            .AsNoTracking()
            .Include(p => p.Elements)
            .FirstOrDefaultAsync(
                p => p.ThemeId == themeId && p.Slug == slug.ToLowerInvariant(),
                cancellationToken)
            ?? throw new KeyNotFoundException($"Page with slug '{slug}' not found in theme {themeId}");

        return ToResponse(page);
    }

    private static PageResponse ToResponse(Page page) =>
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
