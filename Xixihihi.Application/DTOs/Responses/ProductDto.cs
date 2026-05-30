using Xixihihi.Domain.Enums;

namespace Xixihihi.Application.DTOs.Responses;

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsNegotiable { get; set; }
    public ProductCondition Condition { get; set; }
    public ProductStatus Status { get; set; }
    public TransactionType TransactionType { get; set; }
    public Guid CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Size { get; set; }
    public Guid ProvinceId { get; set; }
    public Guid WardId { get; set; }
    public SellerPublicDto Seller { get; set; } = null!;
    public IEnumerable<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
    public DateTime CreatedAt { get; set; }
}
