using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Orders;
using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Orders;

public record UpdateOrderStatusCommand(Guid OrderId, string Status) : IRequest<OrderDto>;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Status).NotEmpty()
            .Must(s => Enum.TryParse<OrderStatus>(s, true, out _))
            .WithMessage("Status must be one of: Pending, Confirmed, Processing, Shipped, Delivered, Cancelled, Refunded, PartiallyRefunded, OnHold.");
    }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        var newStatus = Enum.Parse<OrderStatus>(request.Status, true);

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Delivered or OrderStatus.Refunded)
            throw new BusinessRuleException($"An order in '{order.Status}' status cannot be changed further.");

        // Moving to Shipped is the moment reserved stock is actually deducted from
        // on-hand — see the reserved-vs-on-hand design in InventoryItem.
        if (newStatus == OrderStatus.Shipped && order.Status != OrderStatus.Shipped)
        {
            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariantId is not { } variantId) continue;

                var remaining = item.Quantity;
                var inventoryRows = await _db.InventoryItems
                    .Where(i => i.ProductVariantId == variantId && i.QuantityReserved > 0)
                    .ToListAsync(cancellationToken);

                foreach (var inv in inventoryRows)
                {
                    if (remaining <= 0) break;
                    var take = Math.Min(inv.QuantityReserved, remaining);
                    if (take <= 0) continue;

                    inv.QuantityReserved -= take;
                    inv.QuantityOnHand -= take;
                    remaining -= take;

                    _db.StockMovements.Add(new Domain.Entities.Inventory.StockMovement
                    {
                        InventoryItemId = inv.Id,
                        MovementType = StockMovementType.SaleFulfilled,
                        QuantityChange = -take,
                        QuantityOnHandAfter = inv.QuantityOnHand,
                        ReferenceType = "Order",
                        ReferenceId = order.Id
                    });
                }
            }

            order.ShippedAt = DateTime.UtcNow;
        }

        if (newStatus == OrderStatus.Confirmed && order.ConfirmedAt == null)
            order.ConfirmedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.Delivered)
            order.DeliveredAt = DateTime.UtcNow;

        order.Status = newStatus;
        //order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return OrderMapper.ToDto(order);
    }
}

public record CancelOrderCommand(Guid OrderId) : IRequest<OrderDto>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CancelOrderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        var isOwner = order.Customer?.UserId == _currentUser.UserId;
        if (!isOwner && !_currentUser.IsInRole("Admin") && !_currentUser.IsInRole("StoreAdmin"))
            throw new ForbiddenAppException("You do not have permission to cancel this order.");

        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded)
            throw new BusinessRuleException($"An order in '{order.Status}' status can no longer be cancelled.");

        // Release any reserved (not yet shipped) stock back to availability.
        foreach (var item in order.OrderItems)
        {
            if (item.ProductVariantId is not { } variantId) continue;

            var remaining = item.Quantity;
            var inventoryRows = await _db.InventoryItems
                .Where(i => i.ProductVariantId == variantId && i.QuantityReserved > 0)
                .ToListAsync(cancellationToken);

            foreach (var inv in inventoryRows)
            {
                if (remaining <= 0) break;
                var release = Math.Min(inv.QuantityReserved, remaining);
                if (release <= 0) continue;

                inv.QuantityReserved -= release;
                remaining -= release;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        //order.UpdatedAt = DateTime.UtcNow;

        if (order.Customer != null)
        {
            order.Customer.TotalOrdersCount = Math.Max(0, order.Customer.TotalOrdersCount - 1);
            order.Customer.TotalSpent = Math.Max(0, order.Customer.TotalSpent - order.TotalAmount);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return OrderMapper.ToDto(order);
    }
}
