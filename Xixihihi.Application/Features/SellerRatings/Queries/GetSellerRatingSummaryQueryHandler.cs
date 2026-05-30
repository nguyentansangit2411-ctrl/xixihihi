using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.SellerRatings.Queries;

public class GetSellerRatingSummaryQueryHandler : IRequestHandler<GetSellerRatingSummaryQuery, ApiResponse<SellerRatingSummaryDto>>
{
    private readonly ISellerRatingRepository _sellerRatingRepository;

    public GetSellerRatingSummaryQueryHandler(ISellerRatingRepository sellerRatingRepository)
    {
        _sellerRatingRepository = sellerRatingRepository;
    }

    public async Task<ApiResponse<SellerRatingSummaryDto>> Handle(GetSellerRatingSummaryQuery request, CancellationToken cancellationToken)
    {
        var (totalRatings, averageRating, distribution) = await _sellerRatingRepository.GetRatingSummaryAsync(
            request.SellerId, cancellationToken);

        if (totalRatings == 0)
        {
            return ApiResponse<SellerRatingSummaryDto>.SuccessResponse(new SellerRatingSummaryDto
            {
                SellerId = request.SellerId,
                AverageRating = 0,
                TotalRatings = 0,
                FiveStars = 0,
                FourStars = 0,
                ThreeStars = 0,
                TwoStars = 0,
                OneStar = 0
            }, "Seller has no ratings yet.");
        }

        var summary = new SellerRatingSummaryDto
        {
            SellerId = request.SellerId,
            AverageRating = Math.Round(averageRating, 1),
            TotalRatings = totalRatings,
            FiveStars = distribution.GetValueOrDefault(5, 0),
            FourStars = distribution.GetValueOrDefault(4, 0),
            ThreeStars = distribution.GetValueOrDefault(3, 0),
            TwoStars = distribution.GetValueOrDefault(2, 0),
            OneStar = distribution.GetValueOrDefault(1, 0)
        };

        return ApiResponse<SellerRatingSummaryDto>.SuccessResponse(summary, "Rating summary retrieved successfully.");
    }
}
