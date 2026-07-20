using CommerceCore.Domain.Entities.Marketing;
using CommerceCore.Domain.Entities.Reviews;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers.Generic;

[Route("api/v{version:apiVersion}/coupons")]
public class CouponsController : AdminCrudController<Coupon>
{
    public CouponsController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/wishlists")]
public class WishlistsController : AuthenticatedCrudController<Wishlist>
{
    public WishlistsController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/reviews")]
public class ReviewsController : PublicReadCrudController<Review>
{
    public ReviewsController(IMediator mediator) : base(mediator) { }
}
