namespace Xixihihi.Application.DTOs.Requests;

public class RateSellerRequest
{
    public Guid SellerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
