using NFramework.Persistence.Abstractions.Pagination;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Encapsulates paginated query parameters.
/// </summary>
public record PageableQueryOption<TEntity> : QueryOption<TEntity>, IPageableQuery
{
    /// <summary>
    /// Pagination request parameters.
    /// </summary>
    public Paging Page { get; init; } = new(0, 10);
}
