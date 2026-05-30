namespace Xixihihi.Application.DTOs.Responses;

public class SellerPublicDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ZaloLink { get; set; }
    public string? FacebookLink { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
}
