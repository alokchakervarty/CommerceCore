namespace CommerceCore.Contracts.Orders;

public record OrderItemDto(
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantDisplayName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string PaymentStatus,
    string CurrencyCode,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string ShippingFullName,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string ShippingState,
    string ShippingPostalCode,
    string ShippingCountry,
    DateTime PlacedAt,
    IReadOnlyList<OrderItemDto> Items);

public record CheckoutRequest(Guid ShippingAddressId, Guid? BillingAddressId, string? CouponCode, string PaymentMethod);

public record UpdateOrderStatusRequest(string Status);
