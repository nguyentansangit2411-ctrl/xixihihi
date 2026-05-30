using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.SavedSearches.Queries;

public class GetSavedSearchesQueryHandler : IRequestHandler<GetSavedSearchesQuery, ApiResponse<IEnumerable<SavedSearchDto>>>
{
    private readonly IBaseRepository<SavedSearch> _savedSearchRepository;

    public GetSavedSearchesQueryHandler(IBaseRepository<SavedSearch> savedSearchRepository)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    public async Task<ApiResponse<IEnumerable<SavedSearchDto>>> Handle(GetSavedSearchesQuery request, CancellationToken cancellationToken)
    {
        var savedSearches = await _savedSearchRepository.GetAsync(s => s.UserId == request.UserId, cancellationToken);

        var dtos = savedSearches.Select(s => new SavedSearchDto
        {
            Id = s.Id,
            Name = s.Name,
            SearchTerm = s.SearchTerm,
            CategoryId = s.CategoryId,
            MinPrice = s.MinPrice,
            MaxPrice = s.MaxPrice,
            ProvinceId = s.ProvinceId,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        });

        return ApiResponse<IEnumerable<SavedSearchDto>>.SuccessResponse(dtos, "Saved searches retrieved successfully.");
    }
}
