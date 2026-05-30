using MediatR;
using Microsoft.AspNetCore.Mvc;
using Xixihihi.Application.Features.Locations.Queries;

namespace Xixihihi.API.Controllers;

[Route("api/provinces")]
[ApiController]
public class LocationController : ControllerBase
{
    private readonly IMediator _mediator;

    public LocationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProvinces(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetProvincesQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}/wards")]
    public async Task<IActionResult> GetWardsByProvince(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetWardsByProvinceQuery { ProvinceId = id }, cancellationToken);
        return Ok(response);
    }
}
