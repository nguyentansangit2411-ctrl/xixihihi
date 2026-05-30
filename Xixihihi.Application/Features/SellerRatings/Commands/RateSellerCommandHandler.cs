using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.SellerRatings.Commands;

public class RateSellerCommandHandler : IRequestHandler<RateSellerCommand, ApiResponse<SellerRatingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<SellerRating> _sellerRatingRepository;
    private readonly IBaseRepository<User> _userRepository;

    public RateSellerCommandHandler(
        IBaseRepository<SellerRating> sellerRatingRepository,
        IBaseRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _sellerRatingRepository = sellerRatingRepository;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<SellerRatingDto>> Handle(RateSellerCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra không tự đánh giá chính mình
        if (request.ReviewerId == request.SellerId)
        {
            throw new BusinessException("You cannot rate yourself.");
        }

        // 2. Kiểm tra người bán tồn tại
        var seller = await _userRepository.GetByIdAsync(request.SellerId, cancellationToken);
        if (seller == null)
        {
            throw new NotFoundException($"Seller with ID {request.SellerId} not found.");
        }

        // 3. Kiểm tra xem người đánh giá đã tồn tại chưa
        var reviewer = await _userRepository.GetByIdAsync(request.ReviewerId, cancellationToken);
        if (reviewer == null)
        {
            throw new NotFoundException("Reviewer not found.");
        }

        // 4. Kiểm tra xem đã đánh giá chưa (để tuân thủ Rule 1-1)
        var existingRating = await _sellerRatingRepository.GetFirstOrDefaultAsync(
            sr => sr.SellerId == request.SellerId && sr.ReviewerId == request.ReviewerId, 
            cancellationToken);

        if (existingRating != null)
        {
            throw new BusinessException("You have already rated this seller. Please update your existing rating instead.");
        }

        // 5. Tạo mới Rating
        var ratingEntity = new SellerRating
        {
            SellerId = request.SellerId,
            ReviewerId = request.ReviewerId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        var createdEntity = await _sellerRatingRepository.AddAsync(ratingEntity, cancellationToken);

        // 6. Trả về DTO
        var dto = new SellerRatingDto
        {
            Id = createdEntity.Id,
            SellerId = createdEntity.SellerId,
            ReviewerId = createdEntity.ReviewerId,
            Rating = createdEntity.Rating,
            Comment = createdEntity.Comment,
            CreatedAt = createdEntity.CreatedAt,
            UpdatedAt = createdEntity.UpdatedAt,
            Reviewer = new UserDto
            {
                Id = reviewer.Id,
                DisplayName = reviewer.DisplayName,
                Email = reviewer.Email,
                AvatarUrl = reviewer.AvatarUrl
            }
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<SellerRatingDto>.SuccessResponse(dto, "Rating submitted successfully.");
    }
}

