using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.SavedSearches.Commands;

public class DeleteSavedSearchCommandHandler : IRequestHandler<DeleteSavedSearchCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<SavedSearch> _savedSearchRepository;

    public DeleteSavedSearchCommandHandler(IBaseRepository<SavedSearch> savedSearchRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _savedSearchRepository = savedSearchRepository;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteSavedSearchCommand request, CancellationToken cancellationToken)
    {
        var savedSearch = await _savedSearchRepository.GetByIdAsync(request.Id, cancellationToken);

        if (savedSearch == null)
        {
            throw new NotFoundException($"SavedSearch with ID {request.Id} not found.");
        }

        if (savedSearch.UserId != request.UserId)
        {
            throw new UnauthorizedException("You are not authorized to delete this saved search.");
        }

        await _savedSearchRepository.DeleteAsync(savedSearch, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Saved search deleted successfully.");
    }
}

