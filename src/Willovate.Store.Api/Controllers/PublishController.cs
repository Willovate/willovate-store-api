using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PublishController(IPublishService publishService) : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType<PublicationStatusResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicationStatusResponse>> GetStatus(
        CancellationToken cancellationToken) =>
        Ok(await publishService.GetStatusAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType<PublicationStatusResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicationStatusResponse>> Publish(
        CancellationToken cancellationToken) =>
        Ok(await publishService.PublishAsync(cancellationToken));
}
