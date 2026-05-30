using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Enums;

namespace Xixihihi.Application.Features.Products.Queries;

public class GetProductsQuery : IRequest<ApiResponse<PaginatedResponse<ProductDto>>>
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
    public ProductCondition? Condition { get; set; }
    public TransactionType? TransactionType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsNegotiable { get; set; }

    // Advanced search params
    public string? Brand { get; set; }
    public Guid? SellerId { get; set; }
    public ProductStatus? Status { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
}
