using Asp.Versioning;
using CommerceCore.Application.Features.Cart;
using CommerceCore.Contracts.Cart;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers;

/// <summary>
/// Deliberately NOT [Authorize] — a shopper can build a cart before logging in,
/// identified by the X-Guest-Id header (a GUID the client generates once and
/// persists in a cookie/localStorage). An authenticated caller (Authorization:
/// Bearer ...) uses their own Customer's cart instead; if neither a valid token nor
/// X-Guest-Id is supplied, every action here returns 400 asking for one or the
/// other. Checkout (OrdersController) is where login actually becomes required —
/// see GuestCartMerger for what happens to a guest cart at that point.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<ActionResult<CartResponse>> GetCart(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCartQuery(), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<ActionResult<CartResponse>> AddItem(AddToCartRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AddToCartCommand(request.ProductVariantId, request.Quantity), cancellationToken));

    [HttpPut("{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<ActionResult<CartResponse>> UpdateItem(Guid cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new UpdateCartItemCommand(cartItemId, request.Quantity), cancellationToken));

    [HttpDelete("{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartResponse), 200)]
    public async Task<ActionResult<CartResponse>> RemoveItem(Guid cartItemId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RemoveCartItemCommand(cartItemId), cancellationToken));
}
