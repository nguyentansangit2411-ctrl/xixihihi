using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xixihihi.Application.DTOs.Requests;
using Xixihihi.Application.Features.SellerRatings.Commands;
using Xixihihi.Application.Features.SellerRatings.Queries;

namespace Xixihihi.API.Controllers;

[Route("api/sellers")]
[ApiController]
public class SellerRatingController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellerRatingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Lấy danh sách rating của 1 seller
    [HttpGet("{sellerId:guid}/ratings")]
    public async Task<IActionResult> GetSellerRatings(Guid sellerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetSellerRatingsQuery
        {
            SellerId = sellerId,
            Page = page,
            PageSize = pageSize
        };
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    // Lấy thông kê (Summary) rating của 1 seller
    [HttpGet("{sellerId:guid}/ratings/summary")]
    public async Task<IActionResult> GetSellerRatingSummary(Guid sellerId)
    {
        var response = await _mediator.Send(new GetSellerRatingSummaryQuery { SellerId = sellerId });
        return Ok(response);
    }

    // Thêm rating cho seller
    [HttpPost("{sellerId:guid}/ratings")]
    [Authorize]
    public async Task<IActionResult> RateSeller(Guid sellerId, [FromBody] RateSellerRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var reviewerId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new RateSellerCommand
        {
            ReviewerId = reviewerId,
            SellerId = sellerId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        var response = await _mediator.Send(command);
        // Trả về 201 Created
        return CreatedAtAction(nameof(GetSellerRatings), new { sellerId }, response);
    }

    // Cập nhật rating
    [HttpPut("ratings/{ratingId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateSellerRating(Guid ratingId, [FromBody] UpdateSellerRatingRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var reviewerId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new UpdateSellerRatingCommand
        {
            Id = ratingId,
            ReviewerId = reviewerId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
