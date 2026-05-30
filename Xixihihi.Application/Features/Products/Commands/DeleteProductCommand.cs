using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Products.Commands;

public class DeleteProductCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
}
