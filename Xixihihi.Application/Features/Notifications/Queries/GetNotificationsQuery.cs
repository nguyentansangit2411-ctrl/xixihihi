using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Notifications.Queries;

public class GetNotificationsQuery : IRequest<ApiResponse<PaginatedResponse<NotificationDto>>>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
