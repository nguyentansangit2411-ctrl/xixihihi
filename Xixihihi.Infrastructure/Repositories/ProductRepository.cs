using Microsoft.EntityFrameworkCore;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Domain.Interfaces.Repositories;
using Xixihihi.Infrastructure.Data;

namespace Xixihihi.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsAsync(
        string? search, Guid? categoryId, Guid? provinceId, Guid? wardId,
        ProductCondition? condition, TransactionType? transactionType,
        decimal? minPrice, decimal? maxPrice, bool? isNegotiable,
        string? brand, Guid? sellerId, ProductStatus? status,
        string? sortBy, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Product>()
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .AsQueryable();

        // Mặc định chỉ hiển thị sản phẩm Active, trừ khi truyền status cụ thể hoặc filter theo seller
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        else if (!sellerId.HasValue)
        {
            query = query.Where(p => p.Status == ProductStatus.Active);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(searchLower)
                                  || p.Description.ToLower().Contains(searchLower));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (provinceId.HasValue)
        {
            query = query.Where(p => p.ProvinceId == provinceId.Value);
        }

        if (wardId.HasValue)
        {
            query = query.Where(p => p.WardId == wardId.Value);
        }

        if (condition.HasValue)
        {
            query = query.Where(p => p.Condition == condition.Value);
        }

        if (transactionType.HasValue)
        {
            query = query.Where(p => p.TransactionType == transactionType.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (isNegotiable.HasValue)
        {
            query = query.Where(p => p.IsNegotiable == isNegotiable.Value);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand != null && p.Brand.Contains(brand));
        }

        if (sellerId.HasValue)
        {
            query = query.Where(p => p.SellerId == sellerId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.CreatedAt) // "newest" by default
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product?> GetProductWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Product>()
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
