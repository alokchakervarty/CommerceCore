using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Cart;

/// <summary>
/// Identity.User (who can log in) and Customer (the storefront shopper profile) are
/// deliberately separate tables. This resolves — creating on first use if needed —
/// the Customer row that corresponds to the currently authenticated User, so Cart/
/// Order handlers always have a Customer to attach to.
/// </summary>
internal static class CustomerResolver
{
    public static async Task<Customer> GetOrCreateForCurrentUserAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            throw new UnauthorizedAppException();

        var customer = await db.Customers.FirstOrDefaultAsync(
            c => c.UserId == userId && c.StoreId == tenant.StoreId, cancellationToken);

        if (customer != null) return customer;

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        customer = new Customer
        {
            StoreId = tenant.StoreId,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            IsGuest = false
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        return customer;
    }
}
