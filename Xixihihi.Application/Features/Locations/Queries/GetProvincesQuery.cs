using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Locations.Queries;

public class GetProvincesQuery : IRequest<ApiResponse<List<ProvinceDto>>>
{
}
