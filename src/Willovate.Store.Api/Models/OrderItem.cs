namespace Willovate.Store.Api.Models;

public sealed class OrderItem
{
    public Guid Id { get; init; }
    public Guid OrderId { get; set; }
    public required string MenuItemId { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Order? Order { get; set; }
}
