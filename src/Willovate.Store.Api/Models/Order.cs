namespace Willovate.Store.Api.Models;

public sealed class Order
{
    public Guid Id { get; init; }
    public Guid WebsiteId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public required string OrderMode { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Website? Website { get; set; }
}
