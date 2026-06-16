namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Specifies whether a query is executed as a single query or split into multiple queries.
/// </summary>
public enum QuerySplittingMode
{
    /// <summary>
    /// Uses the default query splitting behavior of the underlying persistence provider.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Splits the query into separate SQL queries per collection inclusion, avoiding cartesian explosion.
    /// </summary>
    SplitQuery = 1,

    /// <summary>
    /// Executes the query as a single SQL statement, joining all data into one result set.
    /// </summary>
    SingleQuery = 2,
}
