using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Products.Queries;

public class GetProductByIdQuery : IRequest<ApiResponse<ProductDto>>
{
    public Guid Id { get; set; }
}
