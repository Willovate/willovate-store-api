using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReservationsController(StoreDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateReservation(CreateReservationRequest request)
    {
        var websiteExists = await context.Websites.AnyAsync(w => w.Id == request.WebsiteId);
        if (!websiteExists)
            return NotFound("Website not found.");

        var reservation = new Reservation
        {
            WebsiteId = request.WebsiteId,
            Date = request.Date,
            Time = request.Time,
            PartySize = request.PartySize,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            SpecialRequests = request.SpecialRequests,
            SeatingPreference = request.SeatingPreference,
            Occasion = request.Occasion,
            Status = "Confirmed",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        return Ok(reservation);
    }

    [HttpGet("{websiteId}")]
    public async Task<IActionResult> GetReservations(Guid websiteId)
    {
        var reservations = await context.Reservations
            .Where(r => r.WebsiteId == websiteId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
            
        return Ok(reservations);
    }
}
