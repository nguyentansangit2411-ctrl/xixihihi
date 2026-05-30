using Microsoft.EntityFrameworkCore;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;
using Xixihihi.Infrastructure.Data;

namespace Xixihihi.Infrastructure.Repositories;

public class SellerRatingRepository : BaseRepository<SellerRating>, ISellerRatingRepository
{
    public SellerRatingRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<SellerRating> Items, int TotalCount)> GetRatingsAsync(
        Guid sellerId, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SellerRatings
            .AsNoTracking()
            .Include(sr => sr.Reviewer)
            .Where(sr => sr.SellerId == sellerId)
            .OrderByDescending(sr => sr.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(int TotalCount, double AverageRating, Dictionary<int, int> Distribution)> GetRatingSummaryAsync(
        Guid sellerId, 
        CancellationToken cancellationToken = default)
    {
        var ratingsQuery = _dbContext.SellerRatings
            .AsNoTracking()
            .Where(sr => sr.SellerId == sellerId);

        var totalCount = await ratingsQuery.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return (0, 0, new Dictionary<int, int>());
        }

        var averageRating = await ratingsQuery.AverageAsync(sr => sr.Rating, cancellationToken);

        var distribution = await ratingsQuery
            .GroupBy(sr => sr.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count, cancellationToken);

        return (totalCount, averageRating, distribution);
    }
}
