using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Enums;

namespace Xixihihi.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<ApiResponse<ProductDto>>
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsNegotiable { get; set; }
    public ProductCondition Condition { get; set; }
    public TransactionType TransactionType { get; set; }
    public Guid CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Size { get; set; }
    public Guid ProvinceId { get; set; }
    public Guid WardId { get; set; }
}
