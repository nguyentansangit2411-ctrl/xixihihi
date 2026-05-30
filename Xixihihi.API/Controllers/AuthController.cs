using MediatR;
using Microsoft.AspNetCore.Mvc;
using Xixihihi.Application.Features.Auth.Commands;

using Microsoft.AspNetCore.RateLimiting;

namespace Xixihihi.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("google")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] LoginWithGoogleCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
