using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class CustomerLoginService(
    StoreDbContext dbContext,
    IPasswordService passwordService) : ICustomerLoginService
{
    public async Task<CustomerResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        // Find customer by normalized email (case-insensitive lookup)
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.NormalizedEmail == normalizedEmail, cancellationToken);

        // Generic failure: email not found OR password incorrect OR inactive
        // Avoids revealing whether an email exists in the system
        if (customer is null || !passwordService.VerifyPassword(request.Password, customer.PasswordHash) || !customer.IsActive)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        return ToResponse(customer);
    }

    private static CustomerResponse ToResponse(Customer customer) => new(
        customer.Id,
        customer.Email,
        customer.FirstName,
        customer.LastName,
        customer.CreatedAt);
}
