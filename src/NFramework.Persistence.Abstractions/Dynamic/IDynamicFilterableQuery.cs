namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Defines dynamic runtime filtering behavior for a query.
/// </summary>
public interface IDynamicFilterableQuery
{
    /// <summary>Filter conditions to apply.</summary>
    ICollection<Filter>? Filters { get; init; }
}
