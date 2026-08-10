using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Billing;
using CommerceCore.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Billing;

public record GetInvoiceQuery(Guid OrderId) : IRequest<InvoiceDto>;

public class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, InvoiceDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetInvoiceQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Order)
            .ThenInclude(o => o!.Customer)
            .FirstOrDefaultAsync(i => i.OrderId == request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Invoice for Order", request.OrderId);

        var isOwner = invoice.Order?.Customer?.UserId == _currentUser.UserId;
        if (!isOwner && !_currentUser.IsInRole("Admin") && !_currentUser.IsInRole("StoreAdmin"))
            throw new ForbiddenAppException("You do not have permission to view this invoice.");

        var orderItems = await _db.OrderItems
            .Where(oi => oi.OrderId == request.OrderId)
            .ToListAsync(cancellationToken);

        return InvoiceMapper.ToDto(invoice, orderItems);
    }
}
