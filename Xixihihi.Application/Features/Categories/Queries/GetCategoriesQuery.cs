using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Categories.Queries;

public class GetCategoriesQuery : IRequest<ApiResponse<IEnumerable<CategoryDto>>>
{
}
