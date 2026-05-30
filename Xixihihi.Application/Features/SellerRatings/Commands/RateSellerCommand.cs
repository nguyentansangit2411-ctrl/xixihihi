using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.SellerRatings.Commands;

public class RateSellerCommand : IRequest<ApiResponse<SellerRatingDto>>
{
    public Guid ReviewerId { get; set; }
    public Guid SellerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
