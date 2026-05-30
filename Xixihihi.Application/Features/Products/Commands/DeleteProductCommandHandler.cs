using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.");
        }

        if (product.SellerId != request.SellerId)
        {
            throw new UnauthorizedException("You do not have permission to delete this product.");
        }

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully.");
    }
}

