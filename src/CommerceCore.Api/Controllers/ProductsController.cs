using Asp.Versioning;
using CommerceCore.Application.Features.Catalog.Products;
using CommerceCore.Contracts.Catalog;
using CommerceCore.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), 200)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] ProductListQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetProductsQuery(query), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetProductByIdQuery(id), cancellationToken));

    [Authorize(Roles = "Admin,StoreAdmin,CatalogManager")]
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), 201)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateProductCommand(
            request.Name, request.ShortDescription, request.Description, request.Sku,
            request.BasePrice, request.CompareAtPrice, request.CostPrice, request.TrackInventory,
            request.CategoryId, request.BrandId, request.ImageUrls), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    [Authorize(Roles = "Admin,StoreAdmin,CatalogManager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProductCommand(
            id, request.Name, request.ShortDescription, request.Description,
            request.BasePrice, request.CompareAtPrice, request.CostPrice, request.TrackInventory,
            request.IsActive, request.IsFeatured, request.CategoryId, request.BrandId), cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,StoreAdmin,CatalogManager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
