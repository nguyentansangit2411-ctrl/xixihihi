using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.Notifications.Queries;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, ApiResponse<PaginatedResponse<NotificationDto>>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<ApiResponse<PaginatedResponse<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _notificationRepository.GetPagedByUserAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            ReferenceId = n.ReferenceId,
            ReferenceLink = n.ReferenceLink,
            Type = (int)n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });

        var paginatedResponse = new PaginatedResponse<NotificationDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return ApiResponse<PaginatedResponse<NotificationDto>>.SuccessResponse(paginatedResponse, "Notifications retrieved successfully.");
    }
}
