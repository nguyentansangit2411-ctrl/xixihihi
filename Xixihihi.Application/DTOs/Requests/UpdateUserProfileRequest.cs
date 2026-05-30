namespace Xixihihi.Application.DTOs.Requests;

public class UpdateUserProfileRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ZaloLink { get; set; }
    public string? FacebookLink { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
}
