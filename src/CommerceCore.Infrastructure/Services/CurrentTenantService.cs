using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CommerceCore.Infrastructure.Services;

/// <summary>
/// Resolves which Store the current request belongs to. Primarily via the
/// "X-Store-Id" header — the deliberate choice for a headless backend, since web,
/// mobile, and third-party frontends don't necessarily share a browser-visible
/// domain/subdomain, so every client can state its store context explicitly and
/// unambiguously.
///
/// When the header is omitted, falls back to Tenant:DefaultStoreId from
/// configuration — a convenience for single-store deployments so every client
/// doesn't need to send the header yet. The header still takes priority whenever
/// it IS supplied, so nothing here needs to change when a second store is added;
/// just start sending X-Store-Id from clients that need a non-default store.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private const string StoreHeaderName = "X-Store-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public Guid StoreId
    {
        get
        {
            var headerValue = _httpContextAccessor.HttpContext?.Request.Headers[StoreHeaderName].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                if (!Guid.TryParse(headerValue, out var headerStoreId))
                    throw new ValidationAppException(new Dictionary<string, string[]>
                    {
                        [StoreHeaderName] = new[] { $"'{StoreHeaderName}' header value is not a valid GUID." }
                    });

                return headerStoreId;
            }

            var configuredValue = _configuration["Tenant:DefaultStoreId"];
            if (!string.IsNullOrWhiteSpace(configuredValue) && Guid.TryParse(configuredValue, out var defaultStoreId))
                return defaultStoreId;

            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [StoreHeaderName] = new[]
                {
                    $"No '{StoreHeaderName}' header was supplied, and no Tenant:DefaultStoreId is configured as a fallback."
                }
            });
        }
    }
}
