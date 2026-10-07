using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WebsitesController(
    IWebsiteService websiteService,
    IThemeService themeService,
    IPageService pageService,
    IPageElementService elementService) : ControllerBase
{
    // Website endpoints
    [HttpGet("{websiteIdOrSlug}")]
    [ProducesResponseType<WebsiteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebsiteResponse>> GetWebsite(
        string websiteIdOrSlug,
        CancellationToken cancellationToken)
    {
        Guid websiteId;
        if (!Guid.TryParse(websiteIdOrSlug, out websiteId))
        {
            if (websiteIdOrSlug.Equals("willovate-store", StringComparison.OrdinalIgnoreCase) || websiteIdOrSlug == "1")
            {
                websiteId = Willovate.Store.Api.Data.SeedData.DefaultWebsiteId;
            }
            else
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid website ID format");
            }
        }

        var website = await websiteService.GetWebsiteAsync(websiteId, cancellationToken);
        return website is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Website not found")
            : Ok(website);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<WebsiteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WebsiteResponse>>> GetWebsites(
        CancellationToken cancellationToken) =>
        Ok(await websiteService.GetWebsitesAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType<WebsiteResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<WebsiteResponse>> CreateWebsite(
        [FromBody] CreateWebsiteRequest request,
        CancellationToken cancellationToken)
    {
        var website = await websiteService.CreateWebsiteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetWebsite), new { websiteIdOrSlug = website.Id }, website);
    }

    [HttpPut("{websiteId}")]
    [ProducesResponseType<WebsiteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebsiteResponse>> UpdateWebsite(
        Guid websiteId,
        [FromBody] UpdateWebsiteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var website = await websiteService.UpdateWebsiteAsync(websiteId, request, cancellationToken);
            return Ok(website);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Website not found");
        }
    }

    [HttpDelete("{websiteId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebsite(
        Guid websiteId,
        CancellationToken cancellationToken)
    {
        try
        {
            await websiteService.DeleteWebsiteAsync(websiteId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Website not found");
        }
    }

    // Theme endpoints
    [HttpGet("{websiteId}/themes")]
    [ProducesResponseType<IReadOnlyList<ThemeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ThemeResponse>>> GetThemesByWebsite(
        Guid websiteId,
        CancellationToken cancellationToken) =>
        Ok(await themeService.GetThemesByWebsiteAsync(websiteId, cancellationToken));

    [HttpGet("themes/{themeId}")]
    [ProducesResponseType<ThemeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThemeResponse>> GetTheme(
        Guid themeId,
        CancellationToken cancellationToken)
    {
        var theme = await themeService.GetThemeAsync(themeId, cancellationToken);
        return theme is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Theme not found")
            : Ok(theme);
    }

    [HttpPost("{websiteId}/themes")]
    [ProducesResponseType<ThemeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThemeResponse>> CreateTheme(
        Guid websiteId,
        [FromBody] CreateThemeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var theme = await themeService.CreateThemeAsync(websiteId, request, cancellationToken);
            return CreatedAtAction(nameof(GetTheme), new { themeId = theme.Id }, theme);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Website not found");
        }
    }

    [HttpPut("themes/{themeId}")]
    [ProducesResponseType<ThemeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThemeResponse>> UpdateTheme(
        Guid themeId,
        [FromBody] UpdateThemeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var theme = await themeService.UpdateThemeAsync(themeId, request, cancellationToken);
            return Ok(theme);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Theme not found");
        }
    }

    [HttpDelete("themes/{themeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTheme(
        Guid themeId,
        CancellationToken cancellationToken)
    {
        try
        {
            await themeService.DeleteThemeAsync(themeId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Theme not found");
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: ex.Message);
        }
    }

    [HttpPost("themes/{themeId}/publish")]
    [ProducesResponseType<ThemeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThemeResponse>> PublishTheme(
        Guid themeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var theme = await themeService.PublishThemeAsync(themeId, cancellationToken);
            return Ok(theme);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Theme not found");
        }
    }

    [HttpPost("themes/{themeId}/duplicate")]
    [ProducesResponseType<ThemeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThemeResponse>> DuplicateTheme(
        Guid themeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var theme = await themeService.DuplicateThemeAsync(themeId, cancellationToken);
            return CreatedAtAction(nameof(GetTheme), new { themeId = theme.Id }, theme);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Theme not found");
        }
    }

    // Page endpoints
    [HttpGet("{themeId}/pages")]
    [ProducesResponseType<IReadOnlyList<PageResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PageResponse>>> GetPagesByTheme(
        Guid themeId,
        CancellationToken cancellationToken) =>
        Ok(await pageService.GetPagesByThemeAsync(themeId, cancellationToken));

    [HttpGet("pages/{pageId}")]
    [ProducesResponseType<PageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageResponse>> GetPage(
        Guid pageId,
        CancellationToken cancellationToken)
    {
        var page = await pageService.GetPageAsync(pageId, cancellationToken);
        return page is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Page not found")
            : Ok(page);
    }

    [HttpPost("{themeId}/pages")]
    [ProducesResponseType<PageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageResponse>> CreatePage(
        Guid themeId,
        [FromBody] CreatePageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var page = await pageService.CreatePageAsync(themeId, request, cancellationToken);
            return CreatedAtAction(nameof(GetPage), new { pageId = page.Id }, page);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Theme not found");
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: ex.Message);
        }
    }

    [HttpPut("pages/{pageId}")]
    [ProducesResponseType<PageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageResponse>> UpdatePage(
        Guid pageId,
        [FromBody] UpdatePageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var page = await pageService.UpdatePageAsync(pageId, request, cancellationToken);
            return Ok(page);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Page not found");
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: ex.Message);
        }
    }

    [HttpDelete("pages/{pageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePage(
        Guid pageId,
        CancellationToken cancellationToken)
    {
        try
        {
            await pageService.DeletePageAsync(pageId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Page not found");
        }
    }

    [HttpGet("{themeId}/pages/{slug}")]
    [ProducesResponseType<PageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageResponse>> GetPageBySlug(
        Guid themeId,
        string slug,
        CancellationToken cancellationToken)
    {
        try
        {
            var page = await pageService.GetPageBySlugAsync(themeId, slug, cancellationToken);
            return Ok(page);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Page not found");
        }
    }

    // Page Element endpoints
    [HttpGet("pages/{pageId}/elements")]
    [ProducesResponseType<IReadOnlyList<PageElementResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PageElementResponse>>> GetElementsByPage(
        Guid pageId,
        CancellationToken cancellationToken) =>
        Ok(await elementService.GetElementsByPageAsync(pageId, cancellationToken));

    [HttpGet("elements/{elementId}")]
    [ProducesResponseType<PageElementResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageElementResponse>> GetElement(
        Guid elementId,
        CancellationToken cancellationToken)
    {
        var element = await elementService.GetElementAsync(elementId, cancellationToken);
        return element is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Element not found")
            : Ok(element);
    }

    [HttpPost("pages/{pageId}/elements")]
    [ProducesResponseType<PageElementResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageElementResponse>> CreateElement(
        Guid pageId,
        [FromBody] CreatePageElementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var element = await elementService.CreateElementAsync(pageId, request, cancellationToken);
            return CreatedAtAction(nameof(GetElement), new { elementId = element.Id }, element);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Page not found");
        }
    }

    [HttpPut("elements/{elementId}")]
    [ProducesResponseType<PageElementResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageElementResponse>> UpdateElement(
        Guid elementId,
        [FromBody] UpdatePageElementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var element = await elementService.UpdateElementAsync(elementId, request, cancellationToken);
            return Ok(element);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Element not found");
        }
    }

    [HttpDelete("elements/{elementId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteElement(
        Guid elementId,
        CancellationToken cancellationToken)
    {
        try
        {
            await elementService.DeleteElementAsync(elementId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Element not found");
        }
    }
}
