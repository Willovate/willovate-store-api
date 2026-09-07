using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class CustomerMicrosoftAuthService(
    StoreDbContext dbContext,
    IMicrosoftTokenValidator tokenValidator,
    IJwtTokenService jwtTokenService) : ICustomerMicrosoftAuthService
{
    public async Task<AuthResponse> AuthenticateMicrosoftUserAsync(
        MicrosoftAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = await tokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new InvalidOperationException("Invalid Microsoft token.");
        }

        var normalizedEmail = payload.Email.Trim().ToUpperInvariant();

        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.NormalizedEmail == normalizedEmail, cancellationToken);

        if (customer is not null)
        {
            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Invalid Microsoft token.");
            }
        }
        else
        {
            var firstName = !string.IsNullOrWhiteSpace(payload.GivenName)
                ? payload.GivenName
                : !string.IsNullOrWhiteSpace(payload.Name)
                    ? payload.Name
                    : "Microsoft";

            var lastName = !string.IsNullOrWhiteSpace(payload.FamilyName)
                ? payload.FamilyName
                : "User";

            // Note: payload.Subject (Microsoft 'sub') is the subject identifier.
            // When dedicated external identity / user login provider mapping is introduced,
            // payload.Subject should be stored in a UserLogin/ExternalAuth entity.
            var now = DateTimeOffset.UtcNow;
            customer = new Customer
            {
                Id = Guid.NewGuid(),
                Email = payload.Email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = "EXTERNAL_AUTH_NO_PASSWORD",
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = now,
                UpdatedAt = now,
                IsActive = true
            };

            dbContext.Customers.Add(customer);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                customer = await dbContext.Customers
                    .FirstOrDefaultAsync(c => c.NormalizedEmail == normalizedEmail, cancellationToken);

                if (customer is null || !customer.IsActive)
                {
                    throw new InvalidOperationException("Invalid Microsoft token.");
                }
            }
        }

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
