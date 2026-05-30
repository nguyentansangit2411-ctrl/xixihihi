using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.SellerRatings.Queries;

public class GetSellerRatingsQueryHandler : IRequestHandler<GetSellerRatingsQuery, ApiResponse<PaginatedResponse<SellerRatingDto>>>
{
    private readonly ISellerRatingRepository _sellerRatingRepository;

    public GetSellerRatingsQueryHandler(ISellerRatingRepository sellerRatingRepository)
    {
        _sellerRatingRepository = sellerRatingRepository;
    }

    public async Task<ApiResponse<PaginatedResponse<SellerRatingDto>>> Handle(GetSellerRatingsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _sellerRatingRepository.GetRatingsAsync(
            request.SellerId, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(sr => new SellerRatingDto
        {
            Id = sr.Id,
            SellerId = sr.SellerId,
            ReviewerId = sr.ReviewerId,
            Rating = sr.Rating,
            Comment = sr.Comment,
            CreatedAt = sr.CreatedAt,
            UpdatedAt = sr.UpdatedAt,
            Reviewer = new UserDto
            {
                Id = sr.Reviewer.Id,
                DisplayName = sr.Reviewer.DisplayName,
                Email = sr.Reviewer.Email,
                AvatarUrl = sr.Reviewer.AvatarUrl
            }
        });

        var paginatedResponse = new PaginatedResponse<SellerRatingDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return ApiResponse<PaginatedResponse<SellerRatingDto>>.SuccessResponse(paginatedResponse, "Ratings retrieved successfully.");
    }
}
