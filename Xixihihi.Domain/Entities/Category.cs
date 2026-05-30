namespace Xixihihi.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
