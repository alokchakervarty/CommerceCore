using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CommerceCore.Infrastructure.Services;

/// <summary>
/// Resolves which Store the current request belongs to via the "X-Store-Id" header.
/// A header (rather than subdomain sniffing) is the deliberate choice for a headless
/// backend: the same API is consumed by web, mobile, and third-party frontends that
/// don't necessarily share a browser-visible domain/subdomain, so every client is
/// required to state its store context explicitly and unambiguously.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private const string StoreHeaderName = "X-Store-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid StoreId
    {
        get
        {
            var headerValue = _httpContextAccessor.HttpContext?.Request.Headers[StoreHeaderName].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(headerValue) || !Guid.TryParse(headerValue, out var storeId))
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    [StoreHeaderName] = new[] { $"A valid '{StoreHeaderName}' header is required on every request." }
                });

            return storeId;
        }
    }
}
