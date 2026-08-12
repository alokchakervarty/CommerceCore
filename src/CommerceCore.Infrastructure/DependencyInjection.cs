using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Infrastructure.Persistence;
using CommerceCore.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace CommerceCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // QuestPDF's Community license is free under $1M USD annual gross revenue —
        // see the licensing note in QuestPdfInvoiceGenerator.cs before relying on
        // this in production above that threshold.
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

        // AppDbContext implements IApplicationDbContext — Application handlers depend
        // only on the interface, never on this concrete registration.
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.Configure<EmailApiSettings>(configuration);
        services.AddHttpClient<IEmailApiSender, EmailApiSender>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ISmsSender, LoggingSmsSender>();
        services.AddScoped<IInvoicePdfGenerator, QuestPdfInvoiceGenerator>();

        return services;
    }
}
