namespace Willovate.Store.Api.Contracts;

public sealed record CreateOrderRequest(
    Guid WebsiteId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? DeliveryAddress,
    string OrderMode,
    List<CreateOrderItemRequest> Items
);

public sealed record CreateOrderItemRequest(
    string MenuItemId,
    string Name,
    decimal Price,
    int Quantity
);
