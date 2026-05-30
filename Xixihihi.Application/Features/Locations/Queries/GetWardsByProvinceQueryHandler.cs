using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;

using Microsoft.Extensions.Caching.Memory;

namespace Xixihihi.Application.Features.Locations.Queries;

public class GetWardsByProvinceQueryHandler : IRequestHandler<GetWardsByProvinceQuery, ApiResponse<List<WardDto>>>
{
    private readonly IBaseRepository<Ward> _wardRepository;
    private readonly IMemoryCache _cache;

    public GetWardsByProvinceQueryHandler(IBaseRepository<Ward> wardRepository, IMemoryCache cache)
    {
        _wardRepository = wardRepository;
        _cache = cache;
    }

    public async Task<ApiResponse<List<WardDto>>> Handle(GetWardsByProvinceQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"wards_by_province_{request.ProvinceId}";
        
        if (!_cache.TryGetValue(cacheKey, out List<WardDto>? dtos))
        {
            var wards = await _wardRepository.GetAsync(x => x.ProvinceId == request.ProvinceId, cancellationToken);
            
            dtos = wards.Select(w => new WardDto
            {
                Id = w.Id,
                Name = w.Name,
                ProvinceId = w.ProvinceId
            })
            .OrderBy(w => w.Name)
            .ToList();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));

            _cache.Set(cacheKey, dtos, cacheEntryOptions);
        }

        return ApiResponse<List<WardDto>>.SuccessResponse(dtos!);
    }
}
