using Asp.Versioning;
using CommerceCore.Application.Features.Catalog.Categories;
using CommerceCore.Contracts.Catalog;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetCategoriesQuery(activeOnly), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken));

    [Authorize(Roles = "Admin,StoreAdmin")]
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCategoryCommand(request.Name, request.Description, request.ImageUrl, request.ParentCategoryId),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    [Authorize(Roles = "Admin,StoreAdmin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateCategoryCommand(id, request.Name, request.Description, request.ImageUrl, request.ParentCategoryId, request.IsActive),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,StoreAdmin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
