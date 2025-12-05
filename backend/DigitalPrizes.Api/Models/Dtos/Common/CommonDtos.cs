namespace DigitalPrizes.Api.Models.Dtos.Common;

/// <summary>
/// Generic paged response wrapper.
/// </summary>
/// <typeparam name="T">Type of items in the response.</typeparam>
public record PagedResponse<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Common paging parameters.
/// </summary>
public record PagingParameters
{
    private const int MaxPageSize = 100;
    private int pageSize = 25;

    public int PageNumber { get; init; } = 1;

    public int PageSize
    {
        get => pageSize;
        init => pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}

/// <summary>
/// Base filter parameters.
/// </summary>
public record FilterParameters : PagingParameters
{
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

/// <summary>
/// Date range filter.
/// </summary>
public record DateRangeFilter
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

/// <summary>
/// Standard API error response.
/// </summary>
public record ApiError
{
    public string Message { get; init; } = string.Empty;
    public string? Code { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}

/// <summary>
/// Standard API response wrapper.
/// </summary>
/// <typeparam name="T">Type of data in the response.</typeparam>
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(string message, string? code = null) => new()
    {
        Success = false,
        Error = new ApiError { Message = message, Code = code }
    };

    public static ApiResponse<T> ValidationFail(Dictionary<string, string[]> errors) => new()
    {
        Success = false,
        Error = new ApiError { Message = "Validation failed", Code = "VALIDATION_ERROR", Errors = errors }
    };
}
