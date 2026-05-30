using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Application.Interfaces;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Products.Commands;

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, ApiResponse<ProductImageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly IBaseRepository<ProductImage> _productImageRepository;
    private readonly IStorageService _storageService;

    public UploadProductImageCommandHandler(
        IProductRepository productRepository, 
        IBaseRepository<ProductImage> productImageRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
        _productImageRepository = productImageRepository;
        _storageService = storageService;
    }

    public async Task<ApiResponse<ProductImageDto>> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductWithDetailsAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.ProductId} not found.");
        }

        if (product.SellerId != request.SellerId)
        {
            throw new UnauthorizedException("You do not have permission to upload images for this product.");
        }

        const int MaxImagesPerProduct = 8;
        if (product.Images.Count >= MaxImagesPerProduct)
        {
            throw new BusinessException($"Tối đa {MaxImagesPerProduct} ảnh cho mỗi sản phẩm.");
        }

        var uploadResult = await _storageService.UploadFileAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);
        
        var maxSortOrder = product.Images.Any() ? product.Images.Max(i => i.SortOrder) : 0;

        var image = new ProductImage
        {
            ProductId = request.ProductId,
            Url = uploadResult.Url,
            CloudinaryPublicId = uploadResult.PublicId,
            SortOrder = maxSortOrder + 1
        };

        await _productImageRepository.AddAsync(image, cancellationToken);

        var dto = new ProductImageDto
        {
            Id = image.Id,
            Url = image.Url,
            SortOrder = image.SortOrder
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<ProductImageDto>.SuccessResponse(dto, "Image uploaded successfully.");
    }
}

