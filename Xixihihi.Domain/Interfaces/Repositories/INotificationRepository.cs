using Xixihihi.Domain.Entities;

namespace Xixihihi.Domain.Interfaces.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
