namespace CommerceCore.Shared.Responses;

/// <summary>
/// Standard pagination envelope for every list/search/filter endpoint.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize) => new()
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}

/// <summary>
/// Common query parameters accepted by every "list" endpoint: paging, free-text search,
/// sorting, and (optionally) a raw filter expression parsed by the query handler.
/// </summary>
public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? Search { get; set; }

    /// <summary>e.g. "name", "-createdDate" (leading "-" = descending)</summary>
    public string? SortBy { get; set; }

    /// <summary>Raw filter string, e.g. "price>=10,price<=100,categoryId=xxxx"</summary>
    public string? Filter { get; set; }
}
