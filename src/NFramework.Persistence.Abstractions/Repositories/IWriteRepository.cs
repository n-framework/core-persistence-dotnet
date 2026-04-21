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
    /// Updates an existing entity.
    /// </summary>
    /// <remarks>
    /// Implementations must check the <see cref="Entity{TId}.RowVersion"/> for optimistic concurrency.
    /// If the version in the data store does not match, a concurrency exception should be thrown.
    /// </remarks>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates an entity.
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
