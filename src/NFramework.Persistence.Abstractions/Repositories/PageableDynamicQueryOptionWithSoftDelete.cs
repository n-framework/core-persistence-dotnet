namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Encapsulates paginated dynamic query parameters with soft delete filtering.
/// </summary>
public record PageableDynamicQueryOptionWithSoftDelete : PageableDynamicQueryOption, IQueryOptionWithSoftDelete
{
    /// <inheritdoc />
    public bool IncludeDeleted { get; init; }
}
