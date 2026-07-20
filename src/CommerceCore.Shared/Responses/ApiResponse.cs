namespace CommerceCore.Shared.Responses;

/// <summary>
/// Uniform envelope returned by every API endpoint in the system, as required
/// by the platform-wide response contract: Success, Message, Data, Errors, TraceId, Timestamp.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Request completed successfully.", string traceId = "")
        => new()
        {
            Success = true,
            Message = message,
            Data = data,
            TraceId = traceId
        };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null, string traceId = "")
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>(),
            TraceId = traceId
        };
}

/// <summary>
/// Non-generic convenience variant for endpoints that return no payload (e.g. DELETE).
/// </summary>
public class ApiResponse : ApiResponse<object?>
{
    public static ApiResponse Ok(string message = "Request completed successfully.", string traceId = "")
        => new()
        {
            Success = true,
            Message = message,
            TraceId = traceId
        };

    public new static ApiResponse Fail(string message, List<string>? errors = null, string traceId = "")
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>(),
            TraceId = traceId
        };
}
