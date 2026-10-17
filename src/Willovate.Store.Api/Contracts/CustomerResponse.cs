namespace Willovate.Store.Api.Contracts;

public sealed record CustomerResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset CreatedAt);
