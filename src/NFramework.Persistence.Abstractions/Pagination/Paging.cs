namespace NFramework.Persistence.Abstractions.Pagination;

/// <summary>
/// Encapsulates pagination request parameters.
/// </summary>
public readonly record struct Paging
{
    /// <summary>Zero-based page index. Must be greater than or equal to 0.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Sonar Analyzer",
        "S2325:Methods and properties that don't access instance data should be 'static'",
        Justification = "Field keyword accesses instance state"
    )]
    public int Index
    {
        get;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Page index must be greater than or equal to 0.");
            }

            field = value;
        }
    }

    /// <summary>Number of items per page. Must be greater than 0.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Sonar Analyzer",
        "S2325:Methods and properties that don't access instance data should be 'static'",
        Justification = "Field keyword accesses instance state"
    )]
    public int Size
    {
        get;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Page size must be greater than 0.");
            }

            field = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Paging"/> struct.
    /// </summary>
    /// <param name="index">Zero-based page index. Must be greater than or equal to 0.</param>
    /// <param name="size">Number of items per page. Must be greater than 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is negative or size is 0 or negative.</exception>
    public Paging(int index, int size)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Page index must be greater than or equal to 0.");
        }

        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Page size must be greater than 0.");
        }

        Index = index;
        Size = size;
    }

    /// <summary>Default paging (0, 10).</summary>
    public static Paging Default => new(0, 10);
}
