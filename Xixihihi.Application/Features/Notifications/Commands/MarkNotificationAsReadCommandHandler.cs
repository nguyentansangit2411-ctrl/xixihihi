using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Notifications.Commands;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<Notification> _notificationRepository;

    public MarkNotificationAsReadCommandHandler(IBaseRepository<Notification> notificationRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
    }

    public async Task<ApiResponse<bool>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (notification == null)
        {
            throw new NotFoundException($"Notification with ID {request.Id} not found.");
        }

        if (notification.UserId != request.UserId)
        {
            throw new UnauthorizedException("You are not authorized to access this notification.");
        }

        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Notification marked as read.");
    }
}

