using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Application.Features.Cart;
using CommerceCore.Contracts.Orders;
using CommerceCore.Domain.Entities.Orders;
using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Orders;

public record CheckoutCommand(Guid ShippingAddressId, Guid? BillingAddressId, string? CouponCode, string PaymentMethod)
    : IRequest<OrderDto>;

public class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.ShippingAddressId).NotEmpty();
        RuleFor(x => x.PaymentMethod).NotEmpty();
    }
}

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenant;

    public CheckoutCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant)
    {
        _db = db;
        _currentUser = currentUser;
        _tenant = tenant;
    }

    public async Task<OrderDto> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var customer = await CustomerResolver.GetOrCreateForCurrentUserAsync(_db, _currentUser, _tenant, cancellationToken);

        var shippingAddress = await _db.Addresses.FirstOrDefaultAsync(
            a => a.Id == request.ShippingAddressId && a.CustomerId == customer.Id, cancellationToken)
            ?? throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.ShippingAddressId)] = new[] { "Invalid shipping address." }
            });

        var billingAddress = request.BillingAddressId.HasValue
            ? await _db.Addresses.FirstOrDefaultAsync(
                a => a.Id == request.BillingAddressId && a.CustomerId == customer.Id, cancellationToken)
            : shippingAddress;

        var countryIds = new[] { shippingAddress.CountryId, billingAddress?.CountryId ?? shippingAddress.CountryId }.Distinct();
        var countryNames = await _db.Set<Domain.Entities.Reference.Country>()
            .Where(c => countryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var shippingCountryName = countryNames.GetValueOrDefault(shippingAddress.CountryId, string.Empty);
        var billingCountryName = countryNames.GetValueOrDefault(billingAddress?.CountryId ?? shippingAddress.CountryId, string.Empty);

        var cartItems = await _db.CartItems
            .Include(ci => ci.Product)
            .Include(ci => ci.ProductVariant)
            .Where(ci => ci.CustomerId == customer.Id)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
            throw new BusinessRuleException("Your cart is empty.");

        // Validate + reserve stock for every line before creating anything, so a
        // single out-of-stock item fails the whole checkout rather than partially.
        var inventoryByVariant = new Dictionary<Guid, List<Domain.Entities.Inventory.InventoryItem>>();
        foreach (var ci in cartItems)
        {
            var items = await _db.InventoryItems
                .Where(i => i.ProductVariantId == ci.ProductVariantId)
                .OrderByDescending(i => i.QuantityOnHand - i.QuantityReserved) // fulfill from the fullest warehouse first
                .ToListAsync(cancellationToken);

            var available = items.Sum(i => i.QuantityOnHand - i.QuantityReserved);
            if (available < ci.Quantity)
                throw new BusinessRuleException($"'{ci.Product?.Name}' no longer has enough stock ({available} available).");

            inventoryByVariant[ci.ProductVariantId] = items;
        }

        var order = new Order
        {
            StoreId = _tenant.StoreId,
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customer.Id,
            Status = OrderStatus.Pending,
            PaymentStatus = OrderPaymentStatus.Pending,
            PaymentMethod = null, // set below once resolved to a snapshot string

            ShippingFullName = shippingAddress.FullName,
            ShippingPhoneNumber = shippingAddress.PhoneNumber,
            ShippingAddressLine1 = shippingAddress.AddressLine1,
            ShippingAddressLine2 = shippingAddress.AddressLine2,
            ShippingCity = shippingAddress.City,
            ShippingState = shippingAddress.State,
            ShippingPostalCode = shippingAddress.PostalCode,
            ShippingCountry = shippingCountryName,

            BillingFullName = billingAddress?.FullName ?? shippingAddress.FullName,
            BillingPhoneNumber = billingAddress?.PhoneNumber ?? shippingAddress.PhoneNumber,
            BillingAddressLine1 = billingAddress?.AddressLine1 ?? shippingAddress.AddressLine1,
            BillingAddressLine2 = billingAddress?.AddressLine2 ?? shippingAddress.AddressLine2,
            BillingCity = billingAddress?.City ?? shippingAddress.City,
            BillingState = billingAddress?.State ?? shippingAddress.State,
            BillingPostalCode = billingAddress?.PostalCode ?? shippingAddress.PostalCode,
            BillingCountry = billingCountryName
        };

        decimal subTotal = 0;

        foreach (var ci in cartItems)
        {
            var unitPrice = ci.ProductVariant?.Price ?? ci.Product?.BasePrice ?? 0;
            var lineTotal = unitPrice * ci.Quantity;
            subTotal += lineTotal;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                ProductVariantId = ci.ProductVariantId,
                ProductNameSnapshot = ci.Product?.Name ?? string.Empty,
                VariantDisplayNameSnapshot = ci.ProductVariant?.IsDefault == true ? null : ci.ProductVariant?.DisplayName,
                SkuSnapshot = ci.ProductVariant?.Sku ?? string.Empty,
                ImageUrlSnapshot = ci.ProductVariant?.ImageUrl,
                UnitPrice = unitPrice,
                Quantity = ci.Quantity
            });

            // Reserve stock (does not touch QuantityOnHand — that happens on fulfillment/shipment).
            var remaining = ci.Quantity;
            foreach (var inv in inventoryByVariant[ci.ProductVariantId])
            {
                if (remaining <= 0) break;
                var availableHere = inv.QuantityOnHand - inv.QuantityReserved;
                var take = Math.Min(availableHere, remaining);
                if (take <= 0) continue;

                inv.QuantityReserved += take;
                remaining -= take;
            }
        }

        // Coupon application is intentionally minimal here: validity/eligibility checks
        // live in the generic Coupon CRUD module; this just records the snapshot if a
        // valid, currently-active coupon code was supplied.
        decimal discountAmount = 0;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(
                c => c.StoreId == _tenant.StoreId && c.Code == request.CouponCode.Trim().ToUpperInvariant() && c.IsActive,
                cancellationToken);

            if (coupon != null && (coupon.EndsAt == null || coupon.EndsAt > DateTime.UtcNow)
                && (coupon.UsageLimitTotal == null || coupon.TimesUsed < coupon.UsageLimitTotal))
            {
                discountAmount = coupon.DiscountType switch
                {
                    Domain.Enums.DiscountType.Percentage => Math.Round(subTotal * (coupon.DiscountValue / 100m), 2),
                    Domain.Enums.DiscountType.FixedAmount => coupon.DiscountValue,
                    _ => 0
                };
                if (coupon.MaxDiscountAmount.HasValue)
                    discountAmount = Math.Min(discountAmount, coupon.MaxDiscountAmount.Value);

                order.CouponId = coupon.Id;
                order.CouponCode = coupon.Code;
                coupon.TimesUsed += 1;

                _db.CouponUsages.Add(new Domain.Entities.Marketing.CouponUsage
                {
                    CouponId = coupon.Id,
                    OrderId = order.Id,
                    CustomerId = customer.Id,
                    DiscountAmountApplied = discountAmount
                });
            }
        }

        order.PaymentMethod = request.PaymentMethod;
        order.SubTotal = subTotal;
        order.DiscountAmount = discountAmount;
        order.ShippingAmount = 0; // shipping-method rate calculation intentionally out of scope for this endpoint
        order.TaxAmount = 0;      // tax calculation intentionally out of scope for this endpoint
        order.TotalAmount = subTotal - discountAmount + order.ShippingAmount + order.TaxAmount;

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);

        customer.TotalOrdersCount += 1;
        customer.TotalSpent += order.TotalAmount;
        customer.LastOrderDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return OrderMapper.ToDto(order);
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";
}
