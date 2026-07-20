using CommerceCore.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommerceCore.Api.Middleware;

/// <summary>
/// Wraps every successful 2xx controller response in the platform-wide ApiResponse
/// envelope, so individual controllers just return plain DTOs/records and never
/// construct the envelope themselves — mirroring how ExceptionMiddleware handles
/// the failure side uniformly. NoContentResult (204, e.g. after a DELETE) is left
/// exactly as-is per HTTP semantics: a 204 must not carry a body.
/// </summary>
public class ApiResponseWrapperFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null) return; // ExceptionMiddleware handles this case

        var traceId = context.HttpContext.TraceIdentifier;

        switch (context.Result)
        {
            case ObjectResult { StatusCode: null or >= 200 and < 300 } objectResult:
                objectResult.Value = ApiResponse<object?>.Ok(objectResult.Value, "Request completed successfully.", traceId);
                break;

            case OkResult:
                context.Result = new ObjectResult(ApiResponse<object?>.Ok(null, "Request completed successfully.", traceId))
                {
                    StatusCode = 200
                };
                break;

            // NoContentResult intentionally left untouched — 204 responses carry no body.
        }
    }
}
