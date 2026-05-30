namespace Xixihihi.Application.DTOs.Responses;

public class SavedSearchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? ProvinceId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
