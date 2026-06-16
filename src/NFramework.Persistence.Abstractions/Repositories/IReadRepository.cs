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
    /// Gets a single entity matching the query options.
    /// </summary>
    /// <returns>The entity on success; <see cref="UnionError.NotFound"/> when no match exists.</returns>
    Task<Rail<TEntity>> GetAsync(QueryOption<TEntity>? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a projected result from a single entity matching the query options.
    /// </summary>
    /// <typeparam name="TResult">Projection result type. Must be a reference type or nullable wrapper for value types.</typeparam>
    /// <returns>The projected result on success; <see cref="UnionError.NotFound"/> when no match exists.</returns>
    Task<Rail<TResult>> GetSelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        QueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    /// <summary>
    /// Retrieves all entities matching the query options.
    /// </summary>
    /// <returns>A read-only list of entities (may be empty).</returns>
    Task<Rail<IReadOnlyList<TEntity>>> GetAllAsync(
        QueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all projected results matching the query options.
    /// </summary>
    /// <returns>A read-only list of projected results (may be empty).</returns>
    Task<Rail<IReadOnlyList<TResult>>> GetAllSelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
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
    /// Retrieves a paginated list of projected results.
    /// </summary>
    /// <returns>A paginated list of projected results.</returns>
    Task<Rail<PaginatedList<TResult>>> GetListSelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
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
