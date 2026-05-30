using Xixihihi.Domain.Entities;

namespace Xixihihi.Domain.Interfaces.Repositories;

public interface ISellerRatingRepository : IBaseRepository<SellerRating>
{
    Task<(IReadOnlyList<SellerRating> Items, int TotalCount)> GetRatingsAsync(
        Guid sellerId, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    Task<(int TotalCount, double AverageRating, Dictionary<int, int> Distribution)> GetRatingSummaryAsync(
        Guid sellerId, 
        CancellationToken cancellationToken = default);
}
