using CommerceCore.Domain.Entities.Cms;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers.Generic;

[Route("api/v{version:apiVersion}/blogs")]
public class BlogsController : PublicReadCrudController<Blog>
{
    public BlogsController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/blog-categories")]
public class BlogCategoriesController : PublicReadCrudController<BlogCategory>
{
    public BlogCategoriesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/pages")]
public class PagesController : PublicReadCrudController<Page>
{
    public PagesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/menus")]
public class MenusController : PublicReadCrudController<Menu>
{
    public MenusController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/menu-items")]
public class MenuItemsController : PublicReadCrudController<MenuItem>
{
    public MenuItemsController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/banners")]
public class BannersController : PublicReadCrudController<Banner>
{
    public BannersController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/faqs")]
public class FaqsController : PublicReadCrudController<Faq>
{
    public FaqsController(IMediator mediator) : base(mediator) { }
}
