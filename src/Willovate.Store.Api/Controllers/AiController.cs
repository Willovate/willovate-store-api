using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AiController(IAiService aiService) : ControllerBase
{
    [HttpPost("chat")]
    public ActionResult<AiChatResponse> Chat([FromBody] AiChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var response = aiService.ProcessMessage(request.Message, request.Context);
        return Ok(response);
    }
}

public record AiChatRequest(string Message, string? Context);

public record AiChatResponse(
    string Reply,
    AiAction? Action);

public record AiAction(
    string Type,           // "update_heading" | "update_description" | "update_button" | "update_banner" | "info"
    string? Text,          // new text content if applicable
    string? ImageUrl,      // new image URL if applicable
    string ElementType     // "heading" | "text" | "button" | "hero"
);
