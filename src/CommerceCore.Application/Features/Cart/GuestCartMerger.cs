using CommerceCore.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Cart;

/// <summary>
/// Folds every CartItem belonging to an anonymous guest (identified by the
/// X-Guest-Id header) into a now-known Customer's cart — called once, right after
/// Register/Login/OTP-login succeeds, so a shopper who added items before logging
/// in doesn't lose them. If the customer already has that same variant in their
/// cart, quantities are added together rather than creating a duplicate line
/// (consistent with AddToCartCommandHandler's own dedupe behavior).
/// </summary>
internal static class GuestCartMerger
{
    public static async Task MergeIfPresentAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant, Guid customerId, CancellationToken cancellationToken)
    {
        if (currentUser.GuestId is not { } guestId)
            return; // no guest cart to merge — the normal case for a returning user who's never shopped anonymously

        var guestItems = await db.CartItems
            .Where(ci => ci.GuestId == guestId)
            .ToListAsync(cancellationToken);

        if (guestItems.Count == 0)
            return;

        var existingCustomerItems = await db.CartItems
            .Where(ci => ci.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        foreach (var guestItem in guestItems)
        {
            var existing = existingCustomerItems.FirstOrDefault(ci => ci.ProductVariantId == guestItem.ProductVariantId);
            if (existing != null)
            {
                existing.Quantity += guestItem.Quantity;
                db.CartItems.Remove(guestItem);
            }
            else
            {
                guestItem.CustomerId = customerId;
                guestItem.GuestId = null;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
