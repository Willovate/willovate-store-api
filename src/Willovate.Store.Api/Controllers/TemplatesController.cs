using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Models;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController(ITemplateService templateService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Template>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Template>>> GetTemplates([FromQuery] string? category, [FromQuery] string? search, CancellationToken cancellationToken)
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
