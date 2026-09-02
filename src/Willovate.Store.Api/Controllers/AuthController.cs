using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    ICustomerRegistrationService registrationService,
    ICustomerLoginService loginService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var customerResponse = await registrationService.RegisterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Register), customerResponse);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Email already registered",
                detail: ex.Message);
        }
    }

    [HttpPost("login")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var customerResponse = await loginService.LoginAsync(request, cancellationToken);
            return Ok(customerResponse);
        }
        catch (InvalidOperationException)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Invalid email or password.");
        }
    }
}
