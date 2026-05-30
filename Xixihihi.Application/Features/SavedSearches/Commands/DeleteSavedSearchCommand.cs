using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.SavedSearches.Commands;

public class DeleteSavedSearchCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
