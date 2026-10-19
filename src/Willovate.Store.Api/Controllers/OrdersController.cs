using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(StoreDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var websiteExists = await context.Websites.AnyAsync(w => w.Id == request.WebsiteId);
        if (!websiteExists)
            return NotFound("Website not found.");

        var totalAmount = request.Items.Sum(i => i.Price * i.Quantity);
        var order = new Order
        {
            WebsiteId = request.WebsiteId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            DeliveryAddress = request.DeliveryAddress,
            OrderMode = request.OrderMode,
            TotalAmount = totalAmount,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                MenuItemId = i.MenuItemId,
                Name = i.Name,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList()
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return Ok(order);
    }

    [HttpGet("{websiteId}")]
    public async Task<IActionResult> GetOrders(Guid websiteId)
    {
        var orders = await context.Orders
            .Include(o => o.Items)
            .Where(o => o.WebsiteId == websiteId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
            
        return Ok(orders);
    }
}
