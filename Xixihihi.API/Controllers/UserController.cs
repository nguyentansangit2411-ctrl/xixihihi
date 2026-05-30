using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xixihihi.Application.DTOs.Requests;
using Xixihihi.Application.Features.Users.Commands;
using Xixihihi.Application.Features.Users.Queries;

namespace Xixihihi.API.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var response = await _mediator.Send(new GetCurrentUserQuery { UserId = userId });
        return Ok(response);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserProfileRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            DisplayName = request.DisplayName,
            AvatarUrl = request.AvatarUrl,
            PhoneNumber = request.PhoneNumber,
            ZaloLink = request.ZaloLink,
            FacebookLink = request.FacebookLink,
            ProvinceId = request.ProvinceId,
            WardId = request.WardId
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var response = await _mediator.Send(new GetUserByIdQuery { UserId = id });
        return Ok(response);
    }
}
