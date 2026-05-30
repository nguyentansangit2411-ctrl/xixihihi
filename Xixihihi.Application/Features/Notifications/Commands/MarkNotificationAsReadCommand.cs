using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Notifications.Commands;

public class MarkNotificationAsReadCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
