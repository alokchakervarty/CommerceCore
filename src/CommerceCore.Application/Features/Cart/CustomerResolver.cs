using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Cart;

/// <summary>
/// Identity.User (who can log in) and Customer (the storefront shopper profile) are
/// deliberately separate tables. This resolves — creating on first use if needed —
/// the Customer row that corresponds to a User, so Cart/Order handlers always have
/// a Customer to attach to.
/// </summary>
internal static class CustomerResolver
{
    public static async Task<Customer> GetOrCreateForCurrentUserAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            throw new UnauthorizedAppException();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        return await GetOrCreateForUserAsync(db, tenant, user, cancellationToken);
    }

    /// <summary>Same resolution, but takes an already-loaded User directly — used by
    /// the Auth handlers (Register/Login/OTP login) right after they resolve/create
    /// the User, so a guest cart merge can happen in the same request without an
    /// extra round trip through ICurrentUserService (whose claims reflect the *previous*
    /// request's token, not the one about to be issued).</summary>
    public static async Task<Customer> GetOrCreateForUserAsync(
        IApplicationDbContext db, ICurrentTenantService tenant, User user, CancellationToken cancellationToken)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(
            c => c.UserId == user.Id && c.StoreId == tenant.StoreId, cancellationToken);

        if (customer != null) return customer;

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
