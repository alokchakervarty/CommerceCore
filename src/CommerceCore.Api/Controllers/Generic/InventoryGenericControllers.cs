using CommerceCore.Domain.Entities.Inventory;
using MediatR;

namespace CommerceCore.Api.Controllers.Generic;

[Microsoft.AspNetCore.Mvc.Route("api/v{version:apiVersion}/warehouses")]
public class WarehousesController : AdminCrudController<Warehouse>
{
    public WarehousesController(IMediator mediator) : base(mediator) { }
}
