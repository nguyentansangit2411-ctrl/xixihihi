using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.Categories.Queries;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, ApiResponse<IEnumerable<CategoryDto>>>
{
    private readonly IBaseRepository<Category> _categoryRepository;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
    private const string CacheKey = "categories_all";

    public GetCategoriesQueryHandler(IBaseRepository<Category> categoryRepository, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _categoryRepository = categoryRepository;
        _cache = cache;
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out IEnumerable<CategoryDto>? cached))
            return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(cached!, "Categories retrieved successfully.");

        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IconUrl = c.IconUrl
        }).ToList();

        _cache.Set(CacheKey, dtos, TimeSpan.FromHours(1));

        return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(dtos, "Categories retrieved successfully.");
    }
}
