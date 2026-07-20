using CommerceCore.Contracts.Orders;
using CommerceCore.Domain.Entities.Orders;

namespace CommerceCore.Application.Features.Orders;

internal static class OrderMapper
{
    public static OrderDto ToDto(Order o) => new(
        o.Id,
        o.OrderNumber,
        o.Status.ToString(),
        o.PaymentStatus.ToString(),
        o.CurrencyCode,
        o.SubTotal,
        o.DiscountAmount,
        o.ShippingAmount,
        o.TaxAmount,
        o.TotalAmount,
        o.ShippingFullName,
        o.ShippingAddressLine1,
        o.ShippingAddressLine2,
        o.ShippingCity,
        o.ShippingState,
        o.ShippingPostalCode,
        o.ShippingCountry,
        o.PlacedAt,
        o.OrderItems.Select(oi => new OrderItemDto(
            oi.ProductId ?? Guid.Empty,
            oi.ProductVariantId,
            oi.ProductNameSnapshot,
            oi.VariantDisplayNameSnapshot,
            oi.SkuSnapshot,
            oi.UnitPrice,
            oi.Quantity,
            oi.LineTotal)).ToList());
}
