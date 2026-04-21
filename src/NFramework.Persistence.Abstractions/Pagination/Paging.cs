namespace NFramework.Persistence.Abstractions.Pagination;

/// <summary>
/// Encapsulates pagination request parameters.
/// </summary>
public readonly record struct Paging
{
    /// <summary>Zero-based page index.</summary>
    public uint Index { get; init; }

    /// <summary>Number of items per page. Must be greater than 0.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Sonar Analyzer",
        "S2325:Methods and properties that don't access instance data should be 'static'",
        Justification = "Field keyword accesses instance state"
    )]
    public uint Size
    {
        get;
        init
        {
            if (value == 0)
            {
                throw new ArgumentException("Page size must be greater than 0.", nameof(value));
            }

            field = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Paging"/> struct.
    /// </summary>
    /// <param name="index">Zero-based page index.</param>
    /// <param name="size">Number of items per page. Must be greater than 0.</param>
    /// <exception cref="ArgumentException">Thrown when size is 0.</exception>
    public Paging(uint index, uint size)
    {
        if (size == 0)
        {
            throw new ArgumentException("Page size must be greater than 0.", nameof(size));
        }

        Index = index;
        Size = size;
    }

    /// <summary>Default paging (0, 10).</summary>
    public static Paging Default => new(0, 10);
}
