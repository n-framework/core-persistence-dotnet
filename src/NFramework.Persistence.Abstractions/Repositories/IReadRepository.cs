using System.Linq.Expressions;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Pagination;
using UnionRailway;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Asynchronous read repository contract returning <see cref="Rail{T}"/> results.
/// </summary>
public interface IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    /// <returns>The entity on success; <see cref="UnionError.NotFound"/> when no match exists.</returns>
    Task<Rail<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity using filter expression.
    /// </summary>
    /// <returns>The entity on success; <see cref="UnionError.NotFound"/> when no match exists.</returns>
    Task<Rail<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all entities matching the query options.
    /// </summary>
    /// <returns>A read-only list of entities (may be empty).</returns>
    Task<Rail<IReadOnlyList<TEntity>>> GetAllAsync(
        QueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a paginated list of entities.
    /// </summary>
    /// <returns>A paginated list of entities.</returns>
    Task<Rail<PaginatedList<TEntity>>> GetListAsync(
        PageableQueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<Rail<bool>> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts entities matching the predicate.
    /// </summary>
    Task<Rail<int>> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );
}
