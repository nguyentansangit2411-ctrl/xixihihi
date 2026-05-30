using Xixihihi.Domain.Enums;

namespace Xixihihi.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    public string? PhoneNumber { get; set; }
    public string? ZaloLink { get; set; }
    public string? FacebookLink { get; set; }

    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
    public Province? Province { get; set; }
    public Ward? Ward { get; set; }

    public ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    
    public ICollection<SellerRating> ReceivedRatings { get; set; } = new List<SellerRating>();
    public ICollection<SellerRating> GivenRatings { get; set; } = new List<SellerRating>();
    
    public ICollection<SavedSearch> SavedSearches { get; set; } = new List<SavedSearch>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

public class ExternalLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
