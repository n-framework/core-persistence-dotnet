using NFramework.Persistence.Abstractions.Entities;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Asynchronous write repository contract for data modification.
/// </summary>
public interface IWriteRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity. Checks RowVersion for concurrency conflicts.
    /// </summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates an entity. If entity exists (by ID), updates it; otherwise inserts new.
    /// </summary>
    Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity. Performs soft delete for <see cref="SoftDeletableEntity{TId}"/>,
    /// hard delete otherwise.
    /// </summary>
    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts multiple entities in batches.
    /// </summary>
    Task<int> BulkAddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities in batches. Checks RowVersion for each.
    /// </summary>
    Task<int> BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities in batches. Respects soft/hard delete per entity type.
    /// </summary>
    Task<int> BulkDeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
