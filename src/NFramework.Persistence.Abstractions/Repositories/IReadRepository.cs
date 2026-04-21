using System.Linq.Expressions;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Pagination;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Asynchronous read repository contract.
/// </summary>
public interface IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity using filter expression.
    /// </summary>
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all entities matching the query options.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        QueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a paginated list of entities.
    /// </summary>
    Task<PaginateResult<TEntity>> GetListAsync(
        PageableQueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts entities matching the predicate.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );
}
