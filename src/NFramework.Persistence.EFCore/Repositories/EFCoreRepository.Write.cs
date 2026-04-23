using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Exceptions;

namespace NFramework.Persistence.EFCore.Repositories;

public abstract partial class EFCoreRepository<TEntity, TId, TContext>
{
    /// <inheritdoc />
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        TEntity? existing =
            await DbSet.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} with ID {entity.Id} not found.");

        if (!ReferenceEquals(existing, entity))
            applyConcurrencyValues(existing, entity);

        return existing;
    }

    private void applyConcurrencyValues(TEntity existing, TEntity callerEntity)
    {
        byte[] callerRowVersion = callerEntity.RowVersion;
        Context.Entry(existing).CurrentValues.SetValues(callerEntity);
        Context.Entry(existing).Property(e => e.RowVersion).OriginalValue = callerRowVersion;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        TEntity? existing = await DbSet.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
        if (existing == null)
            return await AddAsync(entity, cancellationToken).ConfigureAwait(false);

        if (!ReferenceEquals(existing, entity))
            applyConcurrencyValues(existing, entity);

        return existing;
    }

    /// <inheritdoc />
    /// <remarks>
    /// For entities inheriting from SoftDeletableEntity, this operation will be translated
    /// into a soft delete by the SoftDeletionInterceptor during SaveChangesAsync.
    /// </remarks>
    public virtual Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = DbSet.Remove(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual async Task<ICollection<TEntity>> BulkAddAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        var validEntities = entities.Where(e => e != null).ToList();

        if (validEntities.Count == 0)
            return entities;

        await DbSet.AddRangeAsync(validEntities, cancellationToken).ConfigureAwait(false);
        return entities;
    }

    /// <inheritdoc />
    public virtual Task<ICollection<TEntity>> BulkUpdateAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        var validEntities = entities.Where(e => e != null).ToList();

        if (validEntities.Count == 0)
            return Task.FromResult(entities);

        DbSet.UpdateRange(validEntities);
        return Task.FromResult(entities);
    }

    /// <inheritdoc />
    public virtual Task<ICollection<TEntity>> BulkDeleteAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        var validEntities = entities.Where(e => e != null).ToList();

        if (validEntities.Count == 0)
            return Task.FromResult(entities);

        DbSet.RemoveRange(validEntities);
        return Task.FromResult(entities);
    }

    /// <inheritdoc />
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Count > 0 ? ex.Entries[0] : null;
            string entityType = entry?.Metadata.Name ?? typeof(TEntity).Name;
            string entityId = entry?.Property("Id").CurrentValue?.ToString() ?? "Unknown";

            byte[]? currentVersion = entry?.Property("RowVersion").CurrentValue as byte[];
            byte[]? originalVersion = entry?.Property("RowVersion").OriginalValue as byte[];

            throw new ConcurrencyConflictException(
                $"A concurrency conflict was detected for {entityType} with ID {entityId}. The entity was modified by another process.",
                entityType,
                entityId,
                currentVersion,
                originalVersion,
                ex
            );
        }
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already active.</exception>
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already active.");

        _ = await Context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when no transaction is active to commit.</exception>
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("No transaction is active to commit.");

        await Context.Database.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when no transaction is active to roll back.</exception>
    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("No transaction is active to roll back.");

        await Context.Database.RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
    }
}
