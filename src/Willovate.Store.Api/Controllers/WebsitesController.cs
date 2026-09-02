using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WebsitesController(
    IWebsiteService websiteService,
    IPageService pageService,
    IPageElementService elementService) : ControllerBase
{
    // Website endpoints
    [HttpGet("{websiteId}")]
    [ProducesResponseType<WebsiteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebsiteResponse>> GetWebsite(
        Guid websiteId,
        CancellationToken cancellationToken)
    {
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
        return CreatedAtAction(nameof(GetWebsite), new { websiteId = website.Id }, website);
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

    // Page endpoints
    [HttpGet("{websiteId}/pages")]
    [ProducesResponseType<IReadOnlyList<PageResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PageResponse>>> GetPagesByWebsite(
        Guid websiteId,
        CancellationToken cancellationToken) =>
        Ok(await pageService.GetPagesByWebsiteAsync(websiteId, cancellationToken));

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

    [HttpPost("{websiteId}/pages")]
    [ProducesResponseType<PageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageResponse>> CreatePage(
        Guid websiteId,
        [FromBody] CreatePageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var page = await pageService.CreatePageAsync(websiteId, request, cancellationToken);
            return CreatedAtAction(nameof(GetPage), new { pageId = page.Id }, page);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Website not found");
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

    [HttpGet("{websiteId}/pages/{slug}")]
    [ProducesResponseType<PageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageResponse>> GetPageBySlug(
        Guid websiteId,
        string slug,
        CancellationToken cancellationToken)
    {
        try
        {
            var page = await pageService.GetPageBySlugAsync(websiteId, slug, cancellationToken);
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
