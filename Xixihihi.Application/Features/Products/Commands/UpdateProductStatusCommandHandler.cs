using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Enums;
using Xixihihi.Application.Events;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Products.Commands;

public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly IMediator _mediator;

    public UpdateProductStatusCommandHandler(IProductRepository productRepository, IMediator mediator, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
        _mediator = mediator;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.");
        }

        if (product.SellerId != request.SellerId)
        {
            throw new UnauthorizedException("You do not have permission to update this product.");
        }

        bool isNewlyPublished = product.Status != ProductStatus.Active && request.Status == ProductStatus.Active;

        product.Status = request.Status;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product, cancellationToken);

        if (isNewlyPublished)
        {
            await _mediator.Publish(new ProductCreatedEvent
            {
                ProductId = product.Id,
                Title = product.Title,
                CategoryId = product.CategoryId,
                Price = product.Price,
                ProvinceId = product.ProvinceId
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Product status updated successfully.");
    }
}

