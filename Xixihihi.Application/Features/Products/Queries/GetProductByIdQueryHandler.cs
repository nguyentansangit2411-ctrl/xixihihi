using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.Products.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ApiResponse<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ApiResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _productRepository.GetProductWithDetailsAsync(request.Id, cancellationToken);
        if (p == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.");
        }

        var dto = new ProductDto
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
        };

        return ApiResponse<ProductDto>.SuccessResponse(dto, "Product retrieved successfully.");
    }
}
