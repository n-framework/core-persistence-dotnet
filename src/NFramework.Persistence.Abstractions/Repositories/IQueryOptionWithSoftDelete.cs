namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Defines soft delete query options.
/// </summary>
public interface IQueryOptionWithSoftDelete
{
    /// <summary>
    /// When true, includes soft-deleted entities in the query results.
    /// </summary>
    bool IncludeDeleted { get; init; }
}
