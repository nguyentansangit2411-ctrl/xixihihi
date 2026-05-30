using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.Products.Queries;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ApiResponse<PaginatedResponse<ProductDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ApiResponse<PaginatedResponse<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _productRepository.GetProductsAsync(
            request.Search, request.CategoryId, request.ProvinceId, request.WardId,
            request.Condition, request.TransactionType, request.MinPrice, request.MaxPrice,
            request.IsNegotiable, request.Brand, request.SellerId, request.Status,
            request.SortBy, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(p => new ProductDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            Price = p.Price,
            IsNegotiable = p.IsNegotiable,
            Condition = p.Condition,
            Status = p.Status,
            TransactionType = p.TransactionType,
            CategoryId = p.CategoryId,
            Brand = p.Brand,
            ProvinceId = p.ProvinceId,
            WardId = p.WardId,
            CreatedAt = p.CreatedAt,
            Seller = new SellerPublicDto
            {
                Id = p.Seller.Id,
                DisplayName = p.Seller.DisplayName,
                AvatarUrl = p.Seller.AvatarUrl,
                PhoneNumber = p.Seller.PhoneNumber,
                ZaloLink = p.Seller.ZaloLink,
                FacebookLink = p.Seller.FacebookLink,
                ProvinceId = p.Seller.ProvinceId,
                WardId = p.Seller.WardId
            },
            Images = p.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                SortOrder = i.SortOrder
            })
        });

        var paginatedResponse = new PaginatedResponse<ProductDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return ApiResponse<PaginatedResponse<ProductDto>>.SuccessResponse(paginatedResponse, "Products retrieved successfully.");
    }
}
