using CommerceCore.Domain.Entities.Media;
using CommerceCore.Domain.Entities.Notifications;
using CommerceCore.Domain.Entities.SystemAudit;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers.Generic;

[Route("api/v{version:apiVersion}/media")]
public class MediaController : AdminCrudController<MediaAsset>
{
    public MediaController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/notification-templates")]
public class NotificationTemplatesController : AdminCrudController<NotificationTemplate>
{
    public NotificationTemplatesController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/system-settings")]
public class SystemSettingsController : AdminCrudController<SystemSetting>
{
    public SystemSettingsController(IMediator mediator) : base(mediator) { }
}

[Route("api/v{version:apiVersion}/email-templates")]
public class EmailTemplatesController : AdminCrudController<EmailTemplate>
{
    public EmailTemplatesController(IMediator mediator) : base(mediator) { }
}
