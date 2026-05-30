using Xixihihi.Domain.Enums;

namespace Xixihihi.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    // An optional link or resource ID (e.g. ProductId) to navigate to when clicked
    public Guid? ReferenceId { get; set; }
    public string? ReferenceLink { get; set; }
    
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    
    public User User { get; set; } = null!;
}
