using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xixihihi.Application.DTOs.Requests;
using Xixihihi.Application.Features.SavedSearches.Commands;
using Xixihihi.Application.Features.SavedSearches.Queries;

namespace Xixihihi.API.Controllers;

[Route("api/saved-searches")]
[ApiController]
[Authorize]
public class SavedSearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SavedSearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSavedSearches()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var query = new GetSavedSearchesQuery { UserId = userId };
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSavedSearch([FromBody] CreateSavedSearchRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var command = new CreateSavedSearchCommand
        {
            UserId = userId,
            Name = request.Name,
            SearchTerm = request.SearchTerm,
            CategoryId = request.CategoryId,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            ProvinceId = request.ProvinceId
        };

        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSavedSearches), new { }, response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSavedSearch(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var command = new DeleteSavedSearchCommand { Id = id, UserId = userId };
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
