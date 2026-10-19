using Willovate.Store.Api.Controllers;

namespace Willovate.Store.Api.Services;

public interface IAiService
{
    AiChatResponse ProcessMessage(string message, string? context);
}
