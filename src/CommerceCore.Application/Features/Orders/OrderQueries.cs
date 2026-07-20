using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Application.Features.Cart;
using CommerceCore.Contracts.Common;
using CommerceCore.Contracts.Orders;
using CommerceCore.Shared.Exceptions;
using CommerceCore.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Orders;

public record GetMyOrdersQuery(ListQuery Query) : IRequest<PagedResult<OrderDto>>;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenant;

    public GetMyOrdersQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant)
    {
        _db = db;
        _currentUser = currentUser;
        _tenant = tenant;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var customer = await CustomerResolver.GetOrCreateForCurrentUserAsync(_db, _currentUser, _tenant, cancellationToken);

        var query = _db.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customer.Id)
            .OrderByDescending(o => o.PlacedAt);

        var page = request.Query.PageNumber < 1 ? 1 : request.Query.PageNumber;
        var pageSize = request.Query.PageSize is < 1 or > 100 ? 20 : request.Query.PageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<OrderDto>.Create(orders.Select(OrderMapper.ToDto).ToList(), totalCount, page, pageSize);
    }
}

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetOrderByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Order", request.Id);

        // Non-admins may only view their own order; admins bypass this check at the
        // controller level via [Authorize(Roles = "Admin")] on the admin-facing route.
        var isOwner = order.Customer?.UserId == _currentUser.UserId;
        if (!isOwner && !_currentUser.IsInRole("Admin") && !_currentUser.IsInRole("StoreAdmin"))
            throw new ForbiddenAppException("You do not have permission to view this order.");

        return OrderMapper.ToDto(order);
    }
}

public record GetAllOrdersQuery(ListQuery Query, string? Status) : IRequest<PagedResult<OrderDto>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetAllOrdersQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Orders.Include(o => o.OrderItems).Where(o => o.StoreId == _tenant.StoreId);

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<Domain.Enums.OrderStatus>(request.Status, true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        query = query.OrderByDescending(o => o.PlacedAt);

        var page = request.Query.PageNumber < 1 ? 1 : request.Query.PageNumber;
        var pageSize = request.Query.PageSize is < 1 or > 100 ? 20 : request.Query.PageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<OrderDto>.Create(orders.Select(OrderMapper.ToDto).ToList(), totalCount, page, pageSize);
    }
}
