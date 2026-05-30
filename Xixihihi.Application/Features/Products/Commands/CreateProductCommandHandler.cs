using AutoMapper;
using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Exceptions;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Products.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly IBaseRepository<Category> _categoryRepository;
    private readonly IBaseRepository<Province> _provinceRepository;
    private readonly IBaseRepository<Ward> _wardRepository;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(
        IProductRepository productRepository, 
        IBaseRepository<Category> categoryRepository,
        IBaseRepository<Province> provinceRepository,
        IBaseRepository<Ward> wardRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _provinceRepository = provinceRepository;
        _wardRepository = wardRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
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

        var product = new Product
        {
            SellerId = request.SellerId,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            IsNegotiable = request.IsNegotiable,
            Condition = request.Condition,
            Status = ProductStatus.Draft, // Default to Draft when created
            TransactionType = request.TransactionType,
            CategoryId = request.CategoryId,
            Brand = request.Brand,
            Size = request.Size,
            ProvinceId = request.ProvinceId,
            WardId = request.WardId
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var dto = new ProductDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            IsNegotiable = product.IsNegotiable,
            Condition = product.Condition,
            Status = product.Status,
            TransactionType = product.TransactionType,
            CategoryId = product.CategoryId,
            Brand = product.Brand,
            Size = product.Size,
            ProvinceId = product.ProvinceId,
            WardId = product.WardId,
            CreatedAt = product.CreatedAt,
            Images = Enumerable.Empty<ProductImageDto>()
        };

        return ApiResponse<ProductDto>.SuccessResponse(dto, "Product created successfully.");
    }
}

