namespace NFramework.Persistence.Abstractions.Pagination;

/// <summary>
/// Encapsulates pagination request parameters.
/// </summary>
public readonly record struct Paging(uint Index, uint Size)
{
    /// <summary>Zero-based page index.</summary>
    public uint Index { get; init; } = Index;

    /// <summary>Number of items per page.</summary>
    public uint Size { get; init; } = Size;

    /// <summary>Default paging (0, 10).</summary>
    public static Paging Default => new(0, 10);
}
