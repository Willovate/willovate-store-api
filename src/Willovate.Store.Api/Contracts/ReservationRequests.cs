namespace Willovate.Store.Api.Contracts;

public sealed record CreateReservationRequest(
    Guid WebsiteId,
    string Date,
    string Time,
    int PartySize,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? SpecialRequests,
    string? SeatingPreference,
    string? Occasion
);
