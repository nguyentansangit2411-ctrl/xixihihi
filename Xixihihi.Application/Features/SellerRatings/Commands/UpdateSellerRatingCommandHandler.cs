using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.SellerRatings.Commands;

public class UpdateSellerRatingCommandHandler : IRequestHandler<UpdateSellerRatingCommand, ApiResponse<SellerRatingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<SellerRating> _sellerRatingRepository;

    public UpdateSellerRatingCommandHandler(IBaseRepository<SellerRating> sellerRatingRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _sellerRatingRepository = sellerRatingRepository;
    }

    public async Task<ApiResponse<SellerRatingDto>> Handle(UpdateSellerRatingCommand request, CancellationToken cancellationToken)
    {
        var existingRating = await _sellerRatingRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (existingRating == null)
        {
            throw new NotFoundException($"Rating with ID {request.Id} not found.");
        }

        // Kiểm tra quyền sở hữu (chỉ người tạo mới được sửa)
        if (existingRating.ReviewerId != request.ReviewerId)
        {
            throw new UnauthorizedException("You are not authorized to update this rating.");
        }

        existingRating.Rating = request.Rating;
        existingRating.Comment = request.Comment;
        existingRating.UpdatedAt = DateTime.UtcNow;

        await _sellerRatingRepository.UpdateAsync(existingRating, cancellationToken);

        var dto = new SellerRatingDto
        {
            Id = existingRating.Id,
            SellerId = existingRating.SellerId,
            ReviewerId = existingRating.ReviewerId,
            Rating = existingRating.Rating,
            Comment = existingRating.Comment,
            CreatedAt = existingRating.CreatedAt,
            UpdatedAt = existingRating.UpdatedAt
            // Reviewer không cần load ở đây vì DTO có thể null hoặc map partial
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<SellerRatingDto>.SuccessResponse(dto, "Rating updated successfully.");
    }
}

