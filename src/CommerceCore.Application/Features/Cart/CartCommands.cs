using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Cart;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CommerceCore.Application.Features.Cart;

/// <summary>Identifies who a cart belongs to: exactly one of CustomerId (logged in)
/// or GuestId (anonymous, from the X-Guest-Id header) is set.</summary>
internal readonly record struct CartOwner(Guid? CustomerId, Guid? GuestId);

/// <summary>
/// Resolves the current caller's cart owner without requiring login: an
/// authenticated caller resolves to their Customer as before; an anonymous caller
/// resolves to their X-Guest-Id instead, so a shopper can build a cart before ever
/// logging in. Checkout (a separate, still-[Authorize]-protected endpoint) is the
/// point where a login is actually required — see GuestCartMerger for what happens
/// to a guest cart the moment they do log in.
/// </summary>
internal static class CartOwnerResolver
{
    public static async Task<CartOwner> ResolveAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant, CancellationToken cancellationToken)
    {
        if (currentUser.IsAuthenticated && currentUser.UserId != null)
        {
            var customer = await CustomerResolver.GetOrCreateForCurrentUserAsync(db, currentUser, tenant, cancellationToken);
            return new CartOwner(customer.Id, null);
        }

        if (currentUser.GuestId is { } guestId)
            return new CartOwner(null, guestId);

        throw new ValidationAppException(new Dictionary<string, string[]>
        {
            ["X-Guest-Id"] = new[] { "Log in, or send an X-Guest-Id header (a client-generated GUID) to use a cart without logging in." }
        });
    }
}

public record AddToCartCommand(Guid ProductVariantId, int Quantity) : IRequest<CartResponse>;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.ProductVariantId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(1000);
    }
}

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenant;

    public AddToCartCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant)
    {
        _db = db;
        _currentUser = currentUser;
        _tenant = tenant;
    }

    public async Task<CartResponse> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var owner = await CartOwnerResolver.ResolveAsync(_db, _currentUser, _tenant, cancellationToken);

        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.ProductVariantId, cancellationToken)
            ?? throw new NotFoundException("ProductVariant", request.ProductVariantId);

        if (!variant.IsActive)
            throw new BusinessRuleException("This product is no longer available.");

        var availableStock = await _db.InventoryItems
            .Where(i => i.ProductVariantId == variant.Id)
            .SumAsync(i => (int?)(i.QuantityOnHand - i.QuantityReserved), cancellationToken) ?? 0;

        var existing = await _db.CartItems.FirstOrDefaultAsync(
            ci => ci.ProductVariantId == variant.Id
                && (owner.CustomerId != null ? ci.CustomerId == owner.CustomerId : ci.GuestId == owner.GuestId),
            cancellationToken);

        var requestedTotal = (existing?.Quantity ?? 0) + request.Quantity;
        if (requestedTotal > availableStock)
            throw new BusinessRuleException($"Only {availableStock} unit(s) of this product are available.");

        if (existing != null)
        {
            existing.Quantity = requestedTotal;
        }
        else
        {
            _db.CartItems.Add(new CartItem
            {
                CustomerId = owner.CustomerId,
                GuestId = owner.GuestId,
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                Quantity = request.Quantity
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await CartMapper.ToResponseAsync(_db, owner, cancellationToken);
    }
}

public record UpdateCartItemCommand(Guid CartItemId, int Quantity) : IRequest<CartResponse>;

public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(1000);
    }
}

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenant;

    public UpdateCartItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant)
    {
        _db = db;
        _currentUser = currentUser;
        _tenant = tenant;
    }

    public async Task<CartResponse> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var owner = await CartOwnerResolver.ResolveAsync(_db, _currentUser, _tenant, cancellationToken);

        var item = await _db.CartItems.FirstOrDefaultAsync(
            ci => ci.Id == request.CartItemId
                && (owner.CustomerId != null ? ci.CustomerId == owner.CustomerId : ci.GuestId == owner.GuestId),
            cancellationToken)
            ?? throw new NotFoundException("CartItem", request.CartItemId);

        var availableStock = await _db.InventoryItems
            .Where(i => i.ProductVariantId == item.ProductVariantId)
            .SumAsync(i => (int?)(i.QuantityOnHand - i.QuantityReserved), cancellationToken) ?? 0;

        if (request.Quantity > availableStock)
            throw new BusinessRuleException($"Only {availableStock} unit(s) of this product are available.");

        item.Quantity = request.Quantity;
        await _db.SaveChangesAsync(cancellationToken);

        return await CartMapper.ToResponseAsync(_db, owner, cancellationToken);
    }
}

public record RemoveCartItemCommand(Guid CartItemId) : IRequest<CartResponse>;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, CartResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenant;

    public RemoveCartItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant)
    {
        _db = db;
        _currentUser = currentUser;
        _tenant = tenant;
    }

    public async Task<CartResponse> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var owner = await CartOwnerResolver.ResolveAsync(_db, _currentUser, _tenant, cancellationToken);

        var item = await _db.CartItems.FirstOrDefaultAsync(
            ci => ci.Id == request.CartItemId
                && (owner.CustomerId != null ? ci.CustomerId == owner.CustomerId : ci.GuestId == owner.GuestId),
            cancellationToken)
            ?? throw new NotFoundException("CartItem", request.CartItemId);

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);

        return await CartMapper.ToResponseAsync(_db, owner, cancellationToken);
    }
}

internal static class CartMapper
{
    public static async Task<CartResponse> ToResponseAsync(
        IApplicationDbContext db,
        CartOwner owner,
        CancellationToken cancellationToken)
    {
        var rawItems = await db.CartItems
            .Include(ci => ci.Product)
            .Include(ci => ci.ProductVariant)
            .Where(ci => owner.CustomerId != null
                ? ci.CustomerId == owner.CustomerId
                : ci.GuestId == owner.GuestId)
            .ToListAsync(cancellationToken);

        var variantIds = rawItems
            .Select(i => i.ProductVariantId)
            .Distinct()
            .ToList();

        // Available stock
        var stockByVariant = await db.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId))
            .GroupBy(i => i.ProductVariantId)
            .Select(g => new
            {
                ProductVariantId = g.Key,
                Available = g.Sum(i => i.QuantityOnHand - i.QuantityReserved)
            })
            .ToDictionaryAsync(
                x => x.ProductVariantId,
                x => x.Available,
                cancellationToken);

        // Image lookup from ProductImages table
        var imageLookup = await db.ProductImages
    .Where(pi => pi.ProductVariantId.HasValue &&
                 variantIds.Contains(pi.ProductVariantId.Value))
    .GroupBy(pi => pi.ProductVariantId!.Value)
    .Select(g => new
    {
        ProductVariantId = g.Key,
        ImageUrl = g.Select(x => x.Url).FirstOrDefault()
    })
    .ToDictionaryAsync(
        x => x.ProductVariantId,
        x => x.ImageUrl,
        cancellationToken);

        var items = rawItems.Select(ci =>
        {
            var unitPrice = ci.ProductVariant?.Price ?? ci.Product?.BasePrice ?? 0;

            imageLookup.TryGetValue(ci.ProductVariantId, out var imageUrl);

            return new CartItemDto(
                ci.Id,
                ci.ProductId,
                ci.ProductVariantId,
                ci.Product?.Name ?? string.Empty,
                ci.ProductVariant?.IsDefault == true
                    ? null
                    : ci.ProductVariant?.DisplayName,
                imageUrl,
                unitPrice,
                ci.Quantity,
                stockByVariant.TryGetValue(ci.ProductVariantId, out var stock)
                    ? stock
                    : 0);
        }).ToList();

        return new CartResponse(
            items,
            items.Sum(i => i.UnitPrice * i.Quantity),
            items.Sum(i => i.Quantity));
    }
}
