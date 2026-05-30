namespace Xixihihi.Application.DTOs.Requests;

public class CreateSavedSearchRequest
{
    public string Name { get; set; } = string.Empty;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? ProvinceId { get; set; }
}
