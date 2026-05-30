using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Entities;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly IBaseRepository<Category> _categoryRepository;
    private readonly IBaseRepository<Province> _provinceRepository;
    private readonly IBaseRepository<Ward> _wardRepository;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IBaseRepository<Category> categoryRepository,
        IBaseRepository<Province> provinceRepository,
        IBaseRepository<Ward> wardRepository,
        AutoMapper.IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _provinceRepository = provinceRepository;
        _wardRepository = wardRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (categoryExists == null)
            throw new NotFoundException($"Category {request.CategoryId} not found.");

        var provinceExists = await _provinceRepository.GetByIdAsync(request.ProvinceId, cancellationToken);
        if (provinceExists == null)
            throw new NotFoundException($"Province {request.ProvinceId} not found.");

        var wardExists = await _wardRepository.GetByIdAsync(request.WardId, cancellationToken);
        if (wardExists == null)
            throw new NotFoundException($"Ward {request.WardId} not found.");

        var product = await _productRepository.GetProductWithDetailsAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.");
        }

        if (product.SellerId != request.SellerId)
        {
            throw new UnauthorizedException("You do not have permission to update this product.");
        }

        product.Title = request.Title;
        product.Description = request.Description;
        product.Price = request.Price;
        product.IsNegotiable = request.IsNegotiable;
        product.Condition = request.Condition;
        product.TransactionType = request.TransactionType;
        product.CategoryId = request.CategoryId;
        product.Brand = request.Brand;
        product.Size = request.Size;
        product.ProvinceId = request.ProvinceId;
        product.WardId = request.WardId;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product, cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<ProductDto>.SuccessResponse(dto, "Product updated successfully.");
    }
}

