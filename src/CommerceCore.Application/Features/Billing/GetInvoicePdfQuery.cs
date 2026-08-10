using CommerceCore.Application.Common.Interfaces;
using MediatR;

namespace CommerceCore.Application.Features.Billing;

public record GetInvoicePdfQuery(Guid OrderId) : IRequest<byte[]>;

public class GetInvoicePdfQueryHandler : IRequestHandler<GetInvoicePdfQuery, byte[]>
{
    private readonly IMediator _mediator;
    private readonly IInvoicePdfGenerator _pdfGenerator;

    public GetInvoicePdfQueryHandler(IMediator mediator, IInvoicePdfGenerator pdfGenerator)
    {
        _mediator = mediator;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<byte[]> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        // Reuses GetInvoiceQuery so the ownership/authorization check lives in
        // exactly one place, not duplicated between the JSON and PDF endpoints.
        var invoice = await _mediator.Send(new GetInvoiceQuery(request.OrderId), cancellationToken);
        return _pdfGenerator.Generate(invoice);
    }
}
