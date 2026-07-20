using CommerceCore.Domain.Entities.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers.Generic;

[Route("api/v{version:apiVersion}/brands")]
public class BrandsController : PublicReadCrudController<Brand>
{
    public BrandsController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/collections")]
public class CollectionsController : PublicReadCrudController<Collection>
{
    public CollectionsController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/attributes")]
public class AttributesController : AdminCrudController<AttributeDefinition>
{
    public AttributesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/attribute-values")]
public class AttributeValuesController : AdminCrudController<AttributeValue>
{
    public AttributeValuesController(IMediator mediator) : base(mediator) { }
}
