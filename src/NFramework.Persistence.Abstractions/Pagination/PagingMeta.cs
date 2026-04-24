namespace NFramework.Persistence.Abstractions.Pagination;

/// <summary>
/// Encapsulates pagination metadata for navigation.
/// </summary>
public readonly record struct PagingMeta
{
    /// <summary>Paging request parameters.</summary>
    public Paging Paging { get; init; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages { get; init; }

    /// <summary>True if there is a previous page.</summary>
    public bool HasPrevious { get; init; }

    /// <summary>True if there is a next page.</summary>
    public bool HasNext { get; init; }

    public PagingMeta(Paging paging, int totalCount, int totalPages)
    {
        if (totalCount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCount), "Total count cannot be negative.");
        if (totalPages < 0)
            throw new ArgumentOutOfRangeException(nameof(totalPages), "Total pages cannot be negative.");

        Paging = paging;
        TotalCount = totalCount;
        TotalPages = totalPages;
        HasPrevious = paging.Index > 0;
        HasNext = totalPages > 0 && (long)paging.Index + 1 < totalPages;
    }
}
