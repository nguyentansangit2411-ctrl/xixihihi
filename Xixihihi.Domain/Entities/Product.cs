using Xixihihi.Domain.Enums;

namespace Xixihihi.Domain.Entities;

public class Product : BaseEntity
{
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
    public Province? Province { get; set; }
    public Ward? Ward { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public Guid SellerId { get; set; }
    public User Seller { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string CloudinaryPublicId { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public Product Product { get; set; } = null!;
}
