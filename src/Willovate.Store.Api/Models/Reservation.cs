namespace Willovate.Store.Api.Models;

public sealed class Reservation
{
    public Guid Id { get; init; }
    public Guid WebsiteId { get; set; }
    public required string Date { get; set; }
    public required string Time { get; set; }
    public int PartySize { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? SpecialRequests { get; set; }
    public string? SeatingPreference { get; set; }
    public string? Occasion { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public Website? Website { get; set; }
}
