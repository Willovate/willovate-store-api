using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TemplatesController(ITemplateService templateService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TemplateResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TemplateResponse>>> GetTemplates(
        [FromQuery] string businessType = "clothing-store",
        [FromQuery] string? tag = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "popular",
        CancellationToken cancellationToken = default)
    {
        var templates = await templateService.GetTemplatesAsync(
            businessType,
            tag,
            search,
            sortBy,
            cancellationToken);

        return Ok(templates);
    }
}

