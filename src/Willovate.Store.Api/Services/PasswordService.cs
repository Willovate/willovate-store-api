using Microsoft.AspNetCore.Identity;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class PasswordService : IPasswordService
{
    private static readonly Customer PasswordHashingCustomer = new()
    {
        Email = string.Empty,
        NormalizedEmail = string.Empty,
        PasswordHash = string.Empty,
        FirstName = string.Empty,
        LastName = string.Empty
    };

    private readonly PasswordHasher<Customer> passwordHasher = new();

    public string HashPassword(string password) =>
        passwordHasher.HashPassword(PasswordHashingCustomer, password);

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) ||
            passwordHash.StartsWith("EXTERNAL_AUTH_", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            return passwordHasher.VerifyHashedPassword(PasswordHashingCustomer, passwordHash, password)
                != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}