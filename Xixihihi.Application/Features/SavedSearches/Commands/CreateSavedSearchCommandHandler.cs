using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.SavedSearches.Commands;

public class CreateSavedSearchCommandHandler : IRequestHandler<CreateSavedSearchCommand, ApiResponse<SavedSearchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<SavedSearch> _savedSearchRepository;

    public CreateSavedSearchCommandHandler(IBaseRepository<SavedSearch> savedSearchRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _savedSearchRepository = savedSearchRepository;
    }

    public async Task<ApiResponse<SavedSearchDto>> Handle(CreateSavedSearchCommand request, CancellationToken cancellationToken)
    {
        var savedSearch = new SavedSearch
        {
            UserId = request.UserId,
            Name = request.Name,
            SearchTerm = request.SearchTerm,
            CategoryId = request.CategoryId,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            ProvinceId = request.ProvinceId,
            IsActive = true
        };

        var createdEntity = await _savedSearchRepository.AddAsync(savedSearch, cancellationToken);

        var dto = new SavedSearchDto
        {
            Id = createdEntity.Id,
            Name = createdEntity.Name,
            SearchTerm = createdEntity.SearchTerm,
            CategoryId = createdEntity.CategoryId,
            MinPrice = createdEntity.MinPrice,
            MaxPrice = createdEntity.MaxPrice,
            ProvinceId = createdEntity.ProvinceId,
            IsActive = createdEntity.IsActive,
            CreatedAt = createdEntity.CreatedAt
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<SavedSearchDto>.SuccessResponse(dto, "Saved search created successfully.");
    }
}

