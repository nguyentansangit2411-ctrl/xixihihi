using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Locations.Queries;

public class GetWardsByProvinceQuery : IRequest<ApiResponse<List<WardDto>>>
{
    public Guid ProvinceId { get; set; }
}
