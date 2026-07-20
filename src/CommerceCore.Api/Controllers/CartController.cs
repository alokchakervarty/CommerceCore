using Asp.Versioning;
using CommerceCore.Application.Features.Cart;
using CommerceCore.Contracts.Cart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers;

[Authorize]
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
