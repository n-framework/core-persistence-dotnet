namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Represents a single filter condition for dynamic queries.
/// </summary>
public class Filter
{
    /// <summary>Property name on the entity to filter.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Comparison operator.
    /// </summary>
    public FilterOperator Operator { get; set; }

    /// <summary>Value to compare against. Null for null-check operators.</summary>
    public object? Value { get; set; }

    /// <summary>When true, negates the filter condition.</summary>
    public bool IsNot { get; set; }

    /// <summary>When true, string comparisons are case-sensitive.</summary>
    public bool CaseSensitive { get; set; }

    /// <summary>
    /// Logical operator for combining with nested filters.
    /// </summary>
    public FilterLogic? Logic { get; set; }

    /// <summary>Nested filters combined using <see cref="Logic"/>.</summary>
    public ICollection<Filter>? Filters { get; init; }
}
