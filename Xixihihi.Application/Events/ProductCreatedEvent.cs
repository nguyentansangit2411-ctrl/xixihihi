using MediatR;

namespace Xixihihi.Application.Events;

public class ProductCreatedEvent : INotification
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public Guid ProvinceId { get; set; }
}
