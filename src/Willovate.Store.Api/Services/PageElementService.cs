using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class PageElementService(StoreDbContext dbContext) : IPageElementService
{
    public async Task<PageElementResponse?> GetElementAsync(Guid elementId, CancellationToken cancellationToken)
    {
        var element = await dbContext.PageElements
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == elementId, cancellationToken);

        return element is null ? null : ToResponse(element);
    }

    public async Task<IReadOnlyList<PageElementResponse>> GetElementsByPageAsync(Guid pageId, CancellationToken cancellationToken)
    {
        var elements = await dbContext.PageElements
            .AsNoTracking()
            .Where(e => e.PageId == pageId)
            .OrderBy(e => e.DisplayOrder)
            .ToListAsync(cancellationToken);

        return elements.ConvertAll(ToResponse);
    }

    public async Task<PageElementResponse> CreateElementAsync(Guid pageId, CreatePageElementRequest request, CancellationToken cancellationToken)
    {
        // Verify page exists
        var pageExists = await dbContext.Pages.AnyAsync(p => p.Id == pageId, cancellationToken);
        if (!pageExists)
            throw new KeyNotFoundException($"Page {pageId} not found");

        var element = new PageElement
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            ElementType = request.ElementType,
            Name = request.Name,
            DisplayOrder = request.DisplayOrder,
            Properties = request.Properties,
            IsEditable = request.IsEditable,
            IsRequired = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.PageElements.Add(element);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(element);
    }

    public async Task<PageElementResponse> UpdateElementAsync(Guid elementId, UpdatePageElementRequest request, CancellationToken cancellationToken)
    {
        var element = await dbContext.PageElements
            .FirstOrDefaultAsync(e => e.Id == elementId, cancellationToken)
            ?? throw new KeyNotFoundException($"PageElement {elementId} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            element.Name = request.Name;

        if (request.DisplayOrder.HasValue)
            element.DisplayOrder = request.DisplayOrder.Value;

        if (request.Properties is not null)
            element.Properties = request.Properties;

        if (request.IsEditable.HasValue)
            element.IsEditable = request.IsEditable.Value;

        element.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(element);
    }

    public async Task DeleteElementAsync(Guid elementId, CancellationToken cancellationToken)
    {
        var element = await dbContext.PageElements.FindAsync([elementId], cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"PageElement {elementId} not found");

        dbContext.PageElements.Remove(element);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static PageElementResponse ToResponse(PageElement element) =>
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
