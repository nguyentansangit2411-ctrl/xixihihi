using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Products.Commands;

public class DeleteProductImageCommand : IRequest<ApiResponse<bool>>
{
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
    public Guid SellerId { get; set; }
}
