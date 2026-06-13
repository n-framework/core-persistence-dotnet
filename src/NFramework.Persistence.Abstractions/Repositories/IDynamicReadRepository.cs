using System.Diagnostics.CodeAnalysis;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Pagination;
using UnionRailway;

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
    /// <returns>The entity on success; <see cref="UnionError.NotFound"/> when no match exists.</returns>
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    Task<Rail<TEntity>> GetByDynamicAsync(DynamicQueryOption options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities matching the dynamic query options.
    /// </summary>
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    Task<Rail<IReadOnlyList<TEntity>>> GetAllByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a paginated list of entities using dynamic query.
    /// </summary>
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    Task<Rail<PaginatedList<TEntity>>> GetListByDynamicAsync(
        PageableDynamicQueryOption options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if any entity matches the dynamic query options.
    /// </summary>
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    Task<Rail<bool>> AnyByDynamicAsync(DynamicQueryOption options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the dynamic query options.
    /// </summary>
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    Task<Rail<int>> CountByDynamicAsync(DynamicQueryOption options, CancellationToken cancellationToken = default);
}
