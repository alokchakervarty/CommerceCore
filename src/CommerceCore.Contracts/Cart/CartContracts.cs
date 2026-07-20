namespace CommerceCore.Contracts.Cart;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string? VariantDisplayName,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    int AvailableStock);

public record CartResponse(IReadOnlyList<CartItemDto> Items, decimal SubTotal, int TotalItemCount);

public record AddToCartRequest(Guid ProductVariantId, int Quantity);

public record UpdateCartItemRequest(int Quantity);
