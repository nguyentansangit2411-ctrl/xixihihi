using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.SellerRatings.Queries;

public class GetSellerRatingSummaryQuery : IRequest<ApiResponse<SellerRatingSummaryDto>>
{
    public Guid SellerId { get; set; }
}
