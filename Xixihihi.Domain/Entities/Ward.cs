namespace Xixihihi.Domain.Entities;

public class Ward : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid ProvinceId { get; set; }
    public Province Province { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
