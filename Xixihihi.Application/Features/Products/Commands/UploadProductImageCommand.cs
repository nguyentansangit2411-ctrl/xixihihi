using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Products.Commands;

public class UploadProductImageCommand : IRequest<ApiResponse<ProductImageDto>>
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
