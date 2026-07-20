using System.Net;
using System.Text.Json;
using CommerceCore.Shared.Exceptions;
using CommerceCore.Shared.Responses;

namespace CommerceCore.Api.Middleware;

/// <summary>
/// Catches every exception that escapes a controller action and converts it into the
/// platform-wide ApiResponse envelope (Success, Message, Data, Errors, TraceId,
/// Timestamp) with the correct HTTP status code — so no controller ever needs its
/// own try/catch, and every error response, expected or not, has an identical shape.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, response) = exception switch
        {
            ValidationAppException validationEx => (
                validationEx.StatusCode,
                ApiResponse.Fail(
                    "Validation failed.",
                    validationEx.Errors.SelectMany(e => e.Value.Select(msg => $"{e.Key}: {msg}")).ToList(),
                    traceId)),

            NotFoundException notFoundEx => (
                notFoundEx.StatusCode,
                ApiResponse.Fail(notFoundEx.Message, null, traceId)),

            ConflictException conflictEx => (
                conflictEx.StatusCode,
                ApiResponse.Fail(conflictEx.Message, null, traceId)),

            UnauthorizedAppException unauthorizedEx => (
                unauthorizedEx.StatusCode,
                ApiResponse.Fail(unauthorizedEx.Message, null, traceId)),

            ForbiddenAppException forbiddenEx => (
                forbiddenEx.StatusCode,
                ApiResponse.Fail(forbiddenEx.Message, null, traceId)),

            BusinessRuleException businessEx => (
                businessEx.StatusCode,
                ApiResponse.Fail(businessEx.Message, null, traceId)),

            AppException appEx => (
                appEx.StatusCode,
                ApiResponse.Fail(appEx.Message, null, traceId)),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                ApiResponse.Fail(
                    "An unexpected error occurred.",
                    _environment.IsDevelopment() ? new List<string> { exception.ToString() } : null,
                    traceId))
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        else
            _logger.LogWarning("Handled exception ({StatusCode}): {Message}. TraceId: {TraceId}", statusCode, exception.Message, traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionMiddleware>();
}
