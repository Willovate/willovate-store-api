namespace Willovate.Store.Api.Contracts;

public sealed record AuthResponse(
    string AccessToken,
    CustomerResponse Customer);