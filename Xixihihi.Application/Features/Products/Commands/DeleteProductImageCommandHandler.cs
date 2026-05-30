using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Application.Interfaces;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Products.Commands;

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly IBaseRepository<ProductImage> _productImageRepository;
    private readonly IStorageService _storageService;

    public DeleteProductImageCommandHandler(
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

    public async Task<ApiResponse<bool>> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.ProductId} not found.");
        }

        if (product.SellerId != request.SellerId)
        {
            throw new UnauthorizedException("You do not have permission to delete images for this product.");
        }

        var image = await _productImageRepository.GetByIdAsync(request.ImageId, cancellationToken);
        if (image == null || image.ProductId != request.ProductId)
        {
            throw new NotFoundException($"Image with ID {request.ImageId} not found for this product.");
        }

        await _storageService.DeleteFileAsync(image.CloudinaryPublicId, cancellationToken);
        await _productImageRepository.DeleteAsync(image, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Image deleted successfully.");
    }
}

