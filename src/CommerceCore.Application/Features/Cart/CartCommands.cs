using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Cart;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Cart;

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
        var customer = await CustomerResolver.GetOrCreateForCurrentUserAsync(_db, _currentUser, _tenant, cancellationToken);

        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.ProductVariantId, cancellationToken)
            ?? throw new NotFoundException("ProductVariant", request.ProductVariantId);

        if (!variant.IsActive)
            throw new BusinessRuleException("This product is no longer available.");

        var availableStock = await _db.InventoryItems
            .Where(i => i.ProductVariantId == variant.Id)
            .SumAsync(i => (int?)(i.QuantityOnHand - i.QuantityReserved), cancellationToken) ?? 0;

        var existing = await _db.CartItems.FirstOrDefaultAsync(
            ci => ci.CustomerId == customer.Id && ci.ProductVariantId == variant.Id, cancellationToken);

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
                CustomerId = customer.Id,
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                Quantity = request.Quantity
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await CartMapper.ToResponseAsync(_db, customer.Id, cancellationToken);
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
        var customer = await CustomerResolver.GetOrCreateForCurrentUserAsync(_db, _currentUser, _tenant, cancellationToken);

        var item = await _db.CartItems.FirstOrDefaultAsync(
            ci => ci.Id == request.CartItemId && ci.CustomerId == customer.Id, cancellationToken)
            ?? throw new NotFoundException("CartItem", request.CartItemId);

        var availableStock = await _db.InventoryItems
            .Where(i => i.ProductVariantId == item.ProductVariantId)
            .SumAsync(i => (int?)(i.QuantityOnHand - i.QuantityReserved), cancellationToken) ?? 0;

        if (request.Quantity > availableStock)
            throw new BusinessRuleException($"Only {availableStock} unit(s) of this product are available.");

        item.Quantity = request.Quantity;
        await _db.SaveChangesAsync(cancellationToken);

        return await CartMapper.ToResponseAsync(_db, customer.Id, cancellationToken);
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
        var customer = await CustomerResolver.GetOrCreateForCurrentUserAsync(_db, _currentUser, _tenant, cancellationToken);

        var item = await _db.CartItems.FirstOrDefaultAsync(
            ci => ci.Id == request.CartItemId && ci.CustomerId == customer.Id, cancellationToken)
            ?? throw new NotFoundException("CartItem", request.CartItemId);

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);

        return await CartMapper.ToResponseAsync(_db, customer.Id, cancellationToken);
    }
}

internal static class CartMapper
{
    public static async Task<CartResponse> ToResponseAsync(IApplicationDbContext db, Guid customerId, CancellationToken cancellationToken)
    {
        var rawItems = await db.CartItems
            .Include(ci => ci.Product)
            .Include(ci => ci.ProductVariant)
            .Where(ci => ci.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        var variantIds = rawItems.Select(i => i.ProductVariantId).ToList();
        var stockByVariant = await db.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId))
            .GroupBy(i => i.ProductVariantId)
            .Select(g => new { ProductVariantId = g.Key, Available = g.Sum(i => i.QuantityOnHand - i.QuantityReserved) })
            .ToDictionaryAsync(x => x.ProductVariantId, x => x.Available, cancellationToken);

        var items = rawItems.Select(ci =>
        {
            var unitPrice = ci.ProductVariant?.Price ?? ci.Product?.BasePrice ?? 0;
            return new CartItemDto(
                ci.Id,
                ci.ProductId,
                ci.ProductVariantId,
                ci.Product?.Name ?? string.Empty,
                ci.ProductVariant?.IsDefault == true ? null : ci.ProductVariant?.DisplayName,
                ci.ProductVariant?.ImageUrl,
                unitPrice,
                ci.Quantity,
                stockByVariant.TryGetValue(ci.ProductVariantId, out var stock) ? stock : 0);
        }).ToList();

        return new CartResponse(items, items.Sum(i => i.UnitPrice * i.Quantity), items.Sum(i => i.Quantity));
    }
}
