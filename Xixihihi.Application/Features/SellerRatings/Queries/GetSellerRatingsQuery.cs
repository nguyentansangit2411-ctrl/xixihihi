using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.SellerRatings.Queries;

public class GetSellerRatingsQuery : IRequest<ApiResponse<PaginatedResponse<SellerRatingDto>>>
{
    public Guid SellerId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
