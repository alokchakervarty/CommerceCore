using CommerceCore.Domain.Entities.Reference;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers.Generic;

[Route("api/v{version:apiVersion}/countries")]
public class CountriesController : PublicReadCrudController<Country>
{
    public CountriesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/states")]
public class StatesController : PublicReadCrudController<State>
{
    public StatesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/cities")]
public class CitiesController : PublicReadCrudController<City>
{
    public CitiesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/currencies")]
public class CurrenciesController : PublicReadCrudController<Currency>
{
    public CurrenciesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/languages")]
public class LanguagesController : PublicReadCrudController<Language>
{
    public LanguagesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/taxes")]
public class TaxesController : AdminCrudController<Tax>
{
    public TaxesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/shipping-zones")]
public class ShippingZonesController : AdminCrudController<ShippingZone>
{
    public ShippingZonesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/shipping-methods")]
public class ShippingMethodsController : PublicReadCrudController<ShippingMethod>
{
    public ShippingMethodsController(IMediator mediator) : base(mediator) { }
}
