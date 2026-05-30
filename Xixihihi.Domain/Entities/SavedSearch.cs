namespace Xixihihi.Domain.Entities;

public class SavedSearch : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Search criteria
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? ProvinceId { get; set; }
    
    // Name of the saved search chosen by user (optional)
    public string Name { get; set; } = string.Empty;
    
    // Enable/disable push notifications for this filter
    public bool IsActive { get; set; } = true;
    
    public User User { get; set; } = null!;
}
