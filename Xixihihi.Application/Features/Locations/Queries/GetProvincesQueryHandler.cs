using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;

using Microsoft.Extensions.Caching.Memory;

namespace Xixihihi.Application.Features.Locations.Queries;

public class GetProvincesQueryHandler : IRequestHandler<GetProvincesQuery, ApiResponse<List<ProvinceDto>>>
{
    private readonly IBaseRepository<Province> _provinceRepository;
    private readonly IMemoryCache _cache;

    public GetProvincesQueryHandler(IBaseRepository<Province> provinceRepository, IMemoryCache cache)
    {
        _provinceRepository = provinceRepository;
        _cache = cache;
    }

    public async Task<ApiResponse<List<ProvinceDto>>> Handle(GetProvincesQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = "provinces_all";
        
        if (!_cache.TryGetValue(cacheKey, out List<ProvinceDto>? dtos))
        {
            var provinces = await _provinceRepository.GetAsync(x => true, cancellationToken);
            
            dtos = provinces.Select(p => new ProvinceDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code
            })
            .OrderBy(p => p.Name)
            .ToList();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));

            _cache.Set(cacheKey, dtos, cacheEntryOptions);
        }

        return ApiResponse<List<ProvinceDto>>.SuccessResponse(dtos!);
    }
}
