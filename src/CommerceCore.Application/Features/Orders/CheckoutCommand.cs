using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Application.Features.Billing;
using CommerceCore.Application.Features.Cart;
using CommerceCore.Contracts.Orders;
using CommerceCore.Domain.Entities.Billing;
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

        // Build a non-nullable list of country IDs to query the Countries table safely
        var countryIdList = new List<Guid>();
        if (shippingAddress.CountryId.HasValue)
            countryIdList.Add(shippingAddress.CountryId.Value);
        var billingCountryId = billingAddress?.CountryId;
        if (billingCountryId.HasValue)
            countryIdList.Add(billingCountryId.Value);

        var distinctCountryIds = countryIdList.Distinct().ToList();

        var countryNames = distinctCountryIds.Any()
        ? await _db.Set<Domain.Entities.Reference.Country>()
        .Where(c => distinctCountryIds.Contains(c.Id))
        .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken)
        : new Dictionary<Guid, string>();

        string shippingCountryName = string.Empty;
        if (shippingAddress.CountryId.HasValue && countryNames.TryGetValue(shippingAddress.CountryId.Value, out var sname))
            shippingCountryName = sname;

        string billingCountryName = string.Empty;
        var billingCid = billingAddress?.CountryId ?? shippingAddress.CountryId;
        if (billingCid.HasValue && countryNames.TryGetValue(billingCid.Value, out var bname))
            billingCountryName = bname;

        // Indian GST: resolve the seller's registered state so we can decide, once,
        // whether this whole order is intra-state (CGST+SGST) or inter-state (IGST).
        var storeSettings = await _db.StoreSettings.FirstOrDefaultAsync(s => s.StoreId == _tenant.StoreId, cancellationToken);
        string? sellerStateName = null;
        if (storeSettings?.RegisteredStateId is { } registeredStateId)
        {
            sellerStateName = (await _db.Set<Domain.Entities.Reference.State>()
                .FirstOrDefaultAsync(s => s.Id == registeredStateId, cancellationToken))?.Name;
        }
        var isInterState = !GstCalculator.IsSameState(sellerStateName, billingAddress?.State ?? shippingAddress.State);

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

        var isCashOnDelivery = request.PaymentMethod.Equals("Offline", StringComparison.OrdinalIgnoreCase)
            || request.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase)
            || request.PaymentMethod.Equals("Cash on Delivery", StringComparison.OrdinalIgnoreCase);
        var isOnlinePayment = request.PaymentMethod.Equals("Razorpay", StringComparison.OrdinalIgnoreCase)
            || request.PaymentMethod.Equals("Online", StringComparison.OrdinalIgnoreCase);

        var order = new Order
        {
            StoreId = _tenant.StoreId,
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customer.Id,
            // A COD order awaits collection on delivery; a Razorpay order reaches this
            // endpoint only after the client-side gateway signature has been verified.
            Status = OrderStatus.Confirmed,
            PaymentStatus = isOnlinePayment ? OrderPaymentStatus.Paid : OrderPaymentStatus.Pending,
            PaymentMethod = null, // set below once resolved to a snapshot string
            IsInterStateSupply = isInterState,

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
        decimal totalCgst = 0, totalSgst = 0, totalIgst = 0;

        foreach (var ci in cartItems)
        {
            var unitGrossPrice = ci.ProductVariant?.Price ?? ci.Product?.BasePrice ?? 0;
            var gstRate = ci.Product?.GstRatePercentage ?? 0;
            var unitTaxablePrice = GstCalculator.GetTaxableValueFromGross(unitGrossPrice, gstRate);
            var lineTaxableValue = unitTaxablePrice * ci.Quantity;
            subTotal += lineTaxableValue;

            // GST is calculated on the final tax-inclusive price by reverse
            // engineering the taxable value and then splitting tax into CGST/SGST
            // or IGST while preserving the original rounded gross amount.
            var gst = GstCalculator.CalculateFromGross(unitGrossPrice * ci.Quantity, gstRate, isInterState);
            totalCgst += gst.Cgst;
            totalSgst += gst.Sgst;
            totalIgst += gst.Igst;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                ProductVariantId = ci.ProductVariantId,
                ProductNameSnapshot = ci.Product?.Name ?? string.Empty,
                VariantDisplayNameSnapshot = ci.ProductVariant?.IsDefault == true ? null : ci.ProductVariant?.DisplayName,
                SkuSnapshot = ci.ProductVariant?.Sku ?? string.Empty,
                ImageUrlSnapshot = ci.ProductVariant?.ImageUrl,
                HsnCodeSnapshot = ci.Product?.HsnCode,
                UnitPrice = unitTaxablePrice,
                Quantity = ci.Quantity,
                GstRatePercentageSnapshot = gstRate,
                CgstAmount = gst.Cgst,
                SgstAmount = gst.Sgst,
                IgstAmount = gst.Igst,
                TaxAmount = gst.Total
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
        order.TotalCgstAmount = totalCgst;
        order.TotalSgstAmount = totalSgst;
        order.TotalIgstAmount = totalIgst;
        order.TaxAmount = totalCgst + totalSgst + totalIgst;
        order.TotalAmount = subTotal - discountAmount + order.ShippingAmount + order.TaxAmount;

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);

        customer.TotalOrdersCount += 1;
        customer.TotalSpent += order.TotalAmount;
        customer.LastOrderDate = DateTime.UtcNow;

        var invoice = await BuildInvoiceAsync(_db, _tenant.StoreId, order, storeSettings, sellerStateName, isInterState, cancellationToken);
        _db.Invoices.Add(invoice);

        // The invoice number sequence (InvoiceSequence) is a shared per-store row —
        // two simultaneous checkouts could race for it. Retry once, reloading the
        // sequence and regenerating the number, rather than letting an unlucky
        // timing collision fail an otherwise-valid checkout outright.
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            invoice.InvoiceNumber = await InvoiceNumberGenerator.GetNextNumberAsync(_db, _tenant.StoreId, invoice.InvoiceDate, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return OrderMapper.ToDto(order);
    }

    private static async Task<Invoice> BuildInvoiceAsync(
        IApplicationDbContext db, Guid storeId, Order order, Domain.Entities.Stores.StoreSettings? storeSettings,
        string? sellerStateName, bool isInterState, CancellationToken cancellationToken)
    {
        var invoiceDate = DateTime.UtcNow;
        var invoiceNumber = await InvoiceNumberGenerator.GetNextNumberAsync(db, storeId, invoiceDate, cancellationToken);

        return new Invoice
        {
            StoreId = storeId,
            Order = order,
            OrderId = order.Id,
            InvoiceNumber = invoiceNumber,
            FinancialYear = InvoiceNumberGenerator.GetIndianFinancialYear(invoiceDate),
            InvoiceDate = invoiceDate,

            SellerLegalName = storeSettings?.LegalBusinessName ?? "Unregistered Seller",
            SellerGstNumber = storeSettings?.GstNumber,
            SellerPanNumber = storeSettings?.PanNumber,
            SellerAddressLine1 = storeSettings?.RegisteredAddressLine1,
            SellerAddressLine2 = storeSettings?.RegisteredAddressLine2,
            SellerCity = storeSettings?.RegisteredCity,
            SellerState = sellerStateName,
            SellerPostalCode = storeSettings?.RegisteredPostalCode,

            BuyerName = order.BillingFullName,
            BuyerAddressLine1 = order.BillingAddressLine1,
            BuyerAddressLine2 = order.BillingAddressLine2,
            BuyerCity = order.BillingCity,
            BuyerState = order.BillingState,
            BuyerPostalCode = order.BillingPostalCode,
            BuyerPhoneNumber = order.BillingPhoneNumber,

            IsInterStateSupply = isInterState,
            PlaceOfSupplyState = order.ShippingState,

            TaxableValue = order.SubTotal,
            TotalCgstAmount = order.TotalCgstAmount,
            TotalSgstAmount = order.TotalSgstAmount,
            TotalIgstAmount = order.TotalIgstAmount,
            TotalDiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount
        };
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";
}
