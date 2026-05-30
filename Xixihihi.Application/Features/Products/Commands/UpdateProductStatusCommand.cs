using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Enums;

namespace Xixihihi.Application.Features.Products.Commands;

public class UpdateProductStatusCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public ProductStatus Status { get; set; }
}
