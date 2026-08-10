using Asp.Versioning;
using CommerceCore.Application.Features.Billing;
using CommerceCore.Application.Features.Orders;
using CommerceCore.Contracts.Billing;
using CommerceCore.Contracts.Common;
using CommerceCore.Contracts.Orders;
using CommerceCore.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("checkout")]
    [ProducesResponseType(typeof(OrderDto), 201)]
    public async Task<ActionResult<OrderDto>> Checkout(CheckoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CheckoutCommand(request.ShippingAddressId, request.BillingAddressId, request.CouponCode, request.PaymentMethod),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), 200)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetMyOrders([FromQuery] ListQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyOrdersQuery(query), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CancelOrderCommand(id), cancellationToken));

    /// <summary>Returns the GST tax invoice for this order as structured JSON —
    /// same ownership rules as GetById (owner or Admin/StoreAdmin).</summary>
    [HttpGet("{id:guid}/invoice")]
    [ProducesResponseType(typeof(InvoiceDto), 200)]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetInvoiceQuery(id), cancellationToken));

    /// <summary>Returns the same invoice rendered as a downloadable PDF.</summary>
    [HttpGet("{id:guid}/invoice/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetInvoicePdf(Guid id, CancellationToken cancellationToken)
    {
        var pdfBytes = await _mediator.Send(new GetInvoicePdfQuery(id), cancellationToken);
        return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
    }

    [Authorize(Roles = "Admin,StoreAdmin")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), 200)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetAll(
        [FromQuery] ListQuery query, [FromQuery] string? status, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetAllOrdersQuery(query, status), cancellationToken));

    [Authorize(Roles = "Admin,StoreAdmin")]
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new UpdateOrderStatusCommand(id, request.Status), cancellationToken));
}
