using Microsoft.EntityFrameworkCore;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;
using Xixihihi.Infrastructure.Data;

namespace Xixihihi.Infrastructure.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
