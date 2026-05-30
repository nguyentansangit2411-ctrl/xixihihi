using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;

namespace Xixihihi.Domain.Interfaces.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsAsync(
        string? search, Guid? categoryId, Guid? provinceId, Guid? wardId,
        ProductCondition? condition, TransactionType? transactionType,
        decimal? minPrice, decimal? maxPrice, bool? isNegotiable,
        string? brand, Guid? sellerId, ProductStatus? status,
        string? sortBy, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Product?> GetProductWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
