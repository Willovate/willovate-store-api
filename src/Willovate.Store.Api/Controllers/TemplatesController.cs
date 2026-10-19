using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Models;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TemplatesController(ITemplateService templateService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TemplateResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TemplateResponse>>> GetTemplates(
        [FromQuery] string? businessType = "clothing-store",
        [FromQuery] string? category = null,
        [FromQuery] string? tag = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "popular",
        CancellationToken cancellationToken = default)
    {
        var targetType = !string.IsNullOrWhiteSpace(category) ? category : (businessType ?? "clothing-store");
        var templates = await templateService.GetTemplatesAsync(
            targetType,
            tag,
            search,
            sortBy,
            cancellationToken);

        return Ok(templates);
    }

    [HttpGet("all")]
    [ProducesResponseType<IReadOnlyList<Template>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Template>>> GetAllTemplates(
        [FromQuery] string? category,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Template> templates;
        if (!string.IsNullOrWhiteSpace(search))
        {
            templates = await templateService.SearchTemplatesAsync(search, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(category))
        {
            templates = await templateService.GetTemplatesByCategoryAsync(category, cancellationToken);
        }
        else
        {
            templates = await templateService.GetAllTemplatesAsync(cancellationToken);
        }
        return Ok(templates);
    }

    [HttpGet("{templateId}")]
    [ProducesResponseType<Template>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Template>> GetTemplate(string templateId, CancellationToken cancellationToken)
    {
        var template = await templateService.GetTemplateByTemplateIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }
}
