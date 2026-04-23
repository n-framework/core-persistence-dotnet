namespace NFramework.Persistence.Abstractions.Pagination;

/// <summary>
/// Encapsulates pagination metadata for navigation.
/// </summary>
public readonly record struct PagingMeta(Paging Paging, int TotalCount, int TotalPages)
{
    /// <summary>Paging request parameters.</summary>
    public Paging Paging { get; init; } = Paging;

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; init; } = TotalCount;

    /// <summary>Total number of pages.</summary>
    public int TotalPages { get; init; } = TotalPages;

    /// <summary>True if there is a previous page.</summary>
    public bool HasPrevious { get; init; } = Paging.Index > 0;

    /// <summary>True if there is a next page.</summary>
    public bool HasNext { get; init; } = TotalPages > 0 && (long)Paging.Index + 1 < TotalPages;
}
