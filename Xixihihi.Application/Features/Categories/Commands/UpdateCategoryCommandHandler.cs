using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;
using Xixihihi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Xixihihi.Application.Features.Categories.Commands;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<Category> _categoryRepository;
    private readonly IMemoryCache _cache;

    public UpdateCategoryCommandHandler(IBaseRepository<Category> categoryRepository, IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _cache = cache;
    }

    public async Task<ApiResponse<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IconUrl = request.IconUrl;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconUrl = category.IconUrl
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _cache.Remove("categories_all");

        return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category updated successfully.");
    }
}

