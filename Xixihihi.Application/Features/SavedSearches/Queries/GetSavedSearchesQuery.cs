using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.SavedSearches.Queries;

public class GetSavedSearchesQuery : IRequest<ApiResponse<IEnumerable<SavedSearchDto>>>
{
    public Guid UserId { get; set; }
}
