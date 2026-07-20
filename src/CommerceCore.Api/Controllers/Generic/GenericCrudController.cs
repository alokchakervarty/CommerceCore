using Asp.Versioning;
using CommerceCore.Application.Common.Generic;
using CommerceCore.Shared.Entities;
using CommerceCore.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers.Generic;

/// <summary>
/// Generic REST CRUD controller for any BaseEntity-derived type, backed by the
/// GenericCrud&lt;TEntity&gt; MediatR pipeline in the Application layer. A concrete
/// per-entity controller (see e.g. BrandsController) is a two-line class supplying
/// just the route and the [Authorize] policy — everything else is inherited.
///
/// Request/response bodies are the raw Domain entity, serialized directly (with
/// reference-cycle handling configured once in Program.cs) rather than a bespoke DTO
/// per entity. This is a deliberate trade-off for the ~27 reference/CMS/marketing
/// tables: they're simple, admin-managed records with no business-logic projection
/// needed, so a dedicated DTO would just be a maintenance cost with no real benefit —
/// unlike Product/Order/Cart, which get hand-written DTOs precisely because they DO
/// need shaping (computed fields, joins, hidden internals).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
public abstract class GenericCrudController<TEntity> : ControllerBase where TEntity : BaseEntity
{
    protected readonly IMediator Mediator;

    protected GenericCrudController(IMediator mediator)
    {
        Mediator = mediator;
    }

    [HttpGet]
    //[ProducesResponseType(typeof(PagedResult<TEntity>), 200)]
    public virtual async Task<ActionResult<PagedResult<TEntity>>> GetAll(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GenericCrud<TEntity>.GetPaged(pageNumber, pageSize, null, null, null, null),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    //[ProducesResponseType(typeof(TEntity), 200)]
    public virtual async Task<ActionResult<TEntity>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GenericCrud<TEntity>.GetById(id), cancellationToken));

    [HttpPost]
    //[ProducesResponseType(typeof(TEntity), 201)]
    public virtual async Task<ActionResult<TEntity>> Create(TEntity entity, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GenericCrud<TEntity>.Create(entity), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:guid}")]
    //[ProducesResponseType(typeof(TEntity), 200)]
    public virtual async Task<ActionResult<TEntity>> Update(Guid id, TEntity entity, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GenericCrud<TEntity>.Update(id, existing => EntityPropertyCopier.CopyScalarProperties(entity, existing)),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new GenericCrud<TEntity>.Delete(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>Every action requires the Admin/StoreAdmin role — the default for
/// back-office-only data (warehouses, system settings, notification templates, ...).</summary>
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,StoreAdmin")]
public abstract class AdminCrudController<TEntity> : GenericCrudController<TEntity> where TEntity : BaseEntity
{
    protected AdminCrudController(IMediator mediator) : base(mediator) { }
}

/// <summary>Reads are public (storefront-facing reference/CMS data); writes require
/// the Admin/StoreAdmin role.</summary>
public abstract class PublicReadCrudController<TEntity> : GenericCrudController<TEntity> where TEntity : BaseEntity
{
    protected PublicReadCrudController(IMediator mediator) : base(mediator) { }

    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public override Task<ActionResult<PagedResult<TEntity>>> GetAll(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        => base.GetAll(pageNumber, pageSize, cancellationToken);

    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public override Task<ActionResult<TEntity>> GetById(Guid id, CancellationToken cancellationToken)
        => base.GetById(id, cancellationToken);

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,StoreAdmin")]
    public override Task<ActionResult<TEntity>> Create(TEntity entity, CancellationToken cancellationToken)
        => base.Create(entity, cancellationToken);

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,StoreAdmin")]
    public override Task<ActionResult<TEntity>> Update(Guid id, TEntity entity, CancellationToken cancellationToken)
        => base.Update(id, entity, cancellationToken);

    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,StoreAdmin")]
    public override Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => base.Delete(id, cancellationToken);
}

/// <summary>Every action just requires a logged-in user — for customer-owned resources
/// managed through the generic pipeline (e.g. Wishlist, Review) rather than a store-admin-only table.</summary>
[Microsoft.AspNetCore.Authorization.Authorize]
public abstract class AuthenticatedCrudController<TEntity> : GenericCrudController<TEntity> where TEntity : BaseEntity
{
    protected AuthenticatedCrudController(IMediator mediator) : base(mediator) { }
}
