using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Models;

namespace Willovate.Store.Api.Services;

public sealed class CustomerGoogleAuthService(
    StoreDbContext dbContext,
    IGoogleTokenValidator tokenValidator,
    IJwtTokenService jwtTokenService) : ICustomerGoogleAuthService
{
    public async Task<AuthResponse> AuthenticateGoogleUserAsync(
        GoogleAuthRequest request,
        CancellationToken cancellationToken)
    {
        var payload = await tokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (payload is null || !payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new InvalidOperationException("Invalid Google token.");
        }

        var normalizedEmail = payload.Email.Trim().ToUpperInvariant();

        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.NormalizedEmail == normalizedEmail, cancellationToken);

        if (customer is not null)
        {
            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Invalid Google token.");
            }
        }
        else
        {
            var firstName = !string.IsNullOrWhiteSpace(payload.GivenName)
                ? payload.GivenName
                : !string.IsNullOrWhiteSpace(payload.Name)
                    ? payload.Name
                    : "Google";

            var lastName = !string.IsNullOrWhiteSpace(payload.FamilyName)
                ? payload.FamilyName
                : "User";

            // Note: payload.Subject (Google 'sub') is the immutable provider identifier.
            // When dedicated external identity / user login provider mapping is introduced,
            // payload.Subject should be stored in an UserLogin/ExternalAuth entity.
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
            await dbContext.SaveChangesAsync(cancellationToken);
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
