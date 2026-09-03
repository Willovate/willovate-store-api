using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class CustomerRegistrationService(
    StoreDbContext dbContext,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService) : ICustomerRegistrationService
{
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        // Check for duplicate email (matches database unique index logic)
        var existingCustomer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.NormalizedEmail == normalizedEmail, cancellationToken);

        if (existingCustomer is not null)
        {
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
        }

        // Hash password exclusively through IPasswordService
        var passwordHash = passwordService.HashPassword(request.Password);

        // Create new customer with UTC timestamps
        var now = DateTimeOffset.UtcNow;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        };

        // Save through DbContext
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateToken(customer);

        return new AuthResponse(accessToken, ToResponse(customer));
    }

    private static CustomerResponse ToResponse(Customer customer) => new(
        customer.Id,
        customer.Email,
        customer.FirstName,
        customer.LastName,
        customer.CreatedAt);
}
