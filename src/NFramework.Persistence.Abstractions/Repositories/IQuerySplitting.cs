namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Defines query splitting configuration for a query.
/// </summary>
public interface IQuerySplitting
{
    /// <summary>
    /// Gets the query splitting mode.
    /// </summary>
    QuerySplittingMode Splitting { get; init; }
}
