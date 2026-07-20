namespace CommerceCore.Contracts.Common;

/// <summary>Generic list-query parameters accepted by every catalog/order "browse" endpoint.</summary>
public record ListQuery
{
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
