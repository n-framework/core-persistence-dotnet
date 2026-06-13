using NFramework.Persistence.Abstractions.Entities;
using UnionRailway;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Asynchronous write repository contract returning <see cref="Rail{T}"/> results.
/// </summary>
public interface IWriteRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    Task<Rail<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <returns>The updated entity on success; <see cref="UnionError.NotFound"/> when the entity does not exist;
    /// <see cref="UnionError.Conflict"/> on optimistic concurrency failure.</returns>
    Task<Rail<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates an entity.
    /// </summary>
    Task<Rail<TEntity>> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity. Performs soft delete for <see cref="SoftDeletableEntity{TId}"/>,
    /// hard delete otherwise.
    /// </summary>
    Task<Rail<TEntity>> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts multiple entities in batches.
    /// </summary>
    Task<Rail<ICollection<TEntity>>> BulkAddAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates multiple entities in batches. Checks RowVersion for each.
    /// </summary>
    Task<Rail<ICollection<TEntity>>> BulkUpdateAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes multiple entities in batches. Respects soft/hard delete per entity type.
    /// </summary>
    Task<Rail<ICollection<TEntity>>> BulkDeleteAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );
}
