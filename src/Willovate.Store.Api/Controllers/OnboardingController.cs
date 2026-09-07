using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OnboardingController(ITemplateService templateService) : ControllerBase
{
    [HttpPost("select-template")]
    [ProducesResponseType<SelectTemplateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SelectTemplateResponse>> SelectTemplate(
        [FromBody] SelectTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "SessionId is required."
            });
        }

        if (!request.IsBlank && !request.TemplateId.HasValue)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "TemplateId is required when IsBlank is false."
            });
        }

        try
        {
            var response = await templateService.SaveTemplateSelectionAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = ex.Message
            });
        }
    }
}

