using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Users.Commands;

public class UpdateUserProfileCommand : IRequest<ApiResponse<UserDto>>
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ZaloLink { get; set; }
    public string? FacebookLink { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
}
