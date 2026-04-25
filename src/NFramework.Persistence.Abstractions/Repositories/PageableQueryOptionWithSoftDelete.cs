namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Encapsulates paginated query parameters with soft delete filtering.
/// </summary>
public record PageableQueryOptionWithSoftDelete<TEntity> : PageableQueryOption<TEntity>, IQueryOptionWithSoftDelete
{
    /// <inheritdoc />
    public bool IncludeDeleted { get; init; }
}
