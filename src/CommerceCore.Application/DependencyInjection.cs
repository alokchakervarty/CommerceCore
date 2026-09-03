using System.Reflection;
using CommerceCore.Application.Common.Behaviors;
using CommerceCore.Application.Common.Generic;
using CommerceCore.Shared.Entities;
using CommerceCore.Shared.Responses;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        RegisterGenericCrudHandlers(services);
        services.AddValidatorsFromAssembly(assembly);

        // Order matters: unhandled-exception outermost, then logging, then validation
        // closest to the handler so a validation failure is still logged and timed.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    private static void RegisterGenericCrudHandlers(IServiceCollection services)
    {
        var entityTypes = typeof(CommerceCore.Domain.Entities.Cms.Banner).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(BaseEntity).IsAssignableFrom(type));
        var genericCrudType = typeof(GenericCrud<>);

        foreach (var entityType in entityTypes)
        {
            RegisterHandler(services, genericCrudType, entityType, "Create", typeof(IRequestHandler<,>), entityType);
            RegisterHandler(services, genericCrudType, entityType, "GetById", typeof(IRequestHandler<,>), entityType);
            RegisterHandler(services, genericCrudType, entityType, "GetPaged", typeof(IRequestHandler<,>), typeof(PagedResult<>).MakeGenericType(entityType));
            RegisterHandler(services, genericCrudType, entityType, "Update", typeof(IRequestHandler<,>), entityType);

            var deleteRequestType = genericCrudType.MakeGenericType(entityType).GetNestedType("Delete")!.MakeGenericType(entityType);
            var deleteHandlerType = genericCrudType.MakeGenericType(entityType).GetNestedType("DeleteHandler")!.MakeGenericType(entityType);
            services.AddTransient(typeof(IRequestHandler<>).MakeGenericType(deleteRequestType), deleteHandlerType);
        }
    }

    private static void RegisterHandler(
        IServiceCollection services,
        Type genericCrudType,
        Type entityType,
        string requestName,
        Type handlerInterfaceType,
        Type responseType)
    {
        var closedCrudType = genericCrudType.MakeGenericType(entityType);
        var requestType = closedCrudType.GetNestedType(requestName)!.MakeGenericType(entityType);
        var handlerType = closedCrudType.GetNestedType($"{requestName}Handler")!.MakeGenericType(entityType);
        var serviceType = handlerInterfaceType.MakeGenericType(requestType, responseType);
        services.AddTransient(serviceType, handlerType);
    }
}
