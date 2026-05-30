using Xixihihi.Domain.Enums;

namespace Xixihihi.Application.DTOs.Responses;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ZaloLink { get; set; }
    public string? FacebookLink { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
}
