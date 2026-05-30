namespace Xixihihi.Domain.Entities;

public class SellerRating : BaseEntity
{
    public Guid SellerId { get; set; }
    public Guid ReviewerId { get; set; }
    
    // Rating out of 5 stars
    public int Rating { get; set; }
    public string? Comment { get; set; }
    
    public User Seller { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
}
