using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.SellerRatings.Commands;

public class UpdateSellerRatingCommand : IRequest<ApiResponse<SellerRatingDto>>
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
