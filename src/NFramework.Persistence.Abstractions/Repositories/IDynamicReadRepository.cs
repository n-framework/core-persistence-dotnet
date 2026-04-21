using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Pagination;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Read-only dynamic query contract for runtime filtering and sorting.
/// </summary>
/// <typeparam name="TEntity">Entity type inheriting from <see cref="Entity{TId}"/>.</typeparam>
/// <typeparam name="TId">Primary key type implementing <see cref="IEquatable{TId}"/>.</typeparam>
public interface IDynamicReadRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets a single entity matching the dynamic query options.
    /// </summary>
    Task<TEntity?> GetByDynamicAsync(DynamicQueryOption options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities matching the dynamic query options.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a paginated list of entities using dynamic query.
    /// </summary>
    Task<PaginateResult<TEntity>> GetListByDynamicAsync(
        PageableDynamicQueryOption options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if any entity matches the dynamic query options.
    /// </summary>
    Task<bool> AnyByDynamicAsync(DynamicQueryOption options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the dynamic query options.
    /// </summary>
    Task<int> CountByDynamicAsync(DynamicQueryOption options, CancellationToken cancellationToken = default);
}
