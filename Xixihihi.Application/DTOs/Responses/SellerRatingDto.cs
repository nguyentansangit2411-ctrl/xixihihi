namespace Xixihihi.Application.DTOs.Responses;

public class SellerRatingDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid ReviewerId { get; set; }
    
    // Thông tin người đánh giá (không cần thiết lộ toàn bộ info, chỉ cần avatar & name)
    public UserDto Reviewer { get; set; } = null!;
    
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SellerRatingSummaryDto
{
    public Guid SellerId { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    
    // Distribution (Optional)
    public int FiveStars { get; set; }
    public int FourStars { get; set; }
    public int ThreeStars { get; set; }
    public int TwoStars { get; set; }
    public int OneStar { get; set; }
}
