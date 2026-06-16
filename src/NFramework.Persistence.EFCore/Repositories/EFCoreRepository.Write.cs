using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UnionRailway;

namespace NFramework.Persistence.EFCore.Repositories;

public abstract partial class EFCoreRepository<TEntity, TId, TContext>
{
    /// <inheritdoc />
    public virtual async Task<Rail<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<Rail<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        TEntity? existing = await DbSet.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
        if (existing is null)
            return new UnionError.NotFound(typeof(TEntity).Name);

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
    public virtual async Task<Rail<TEntity>> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
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
    public virtual Task<Rail<TEntity>> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = DbSet.Remove(entity);
        return Task.FromResult<Rail<TEntity>>(entity);
    }

    /// <inheritdoc />
    public virtual async Task<Rail<ICollection<TEntity>>> BulkAddAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (entities.Any(e => e == null))
            throw new ArgumentException("Collection contains null entities.", nameof(entities));

        if (entities.Count == 0)
            return entities;

        foreach (var chunk in entities.Chunk(MaxBatchSize))
        {
            await DbSet.AddRangeAsync(chunk, cancellationToken).ConfigureAwait(false);
            Rail<int> saveResult = await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (!saveResult.IsSuccess(out _, out _))
                return saveResult.Error!.Value;
        }

        return entities;
    }

    /// <inheritdoc />
    public virtual async Task<Rail<ICollection<TEntity>>> BulkUpdateAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (entities.Any(e => e == null))
            throw new ArgumentException("Collection contains null entities.", nameof(entities));

        if (entities.Count == 0)
            return entities;

        foreach (var chunk in entities.Chunk(MaxBatchSize))
        {
            DbSet.UpdateRange(chunk);
            Rail<int> saveResult = await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (!saveResult.IsSuccess(out _, out _))
                return saveResult.Error!.Value;
        }

        return entities;
    }

    /// <inheritdoc />
    public virtual async Task<Rail<ICollection<TEntity>>> BulkDeleteAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (entities.Any(e => e == null))
            throw new ArgumentException("Collection contains null entities.", nameof(entities));

        if (entities.Count == 0)
            return entities;

        foreach (var chunk in entities.Chunk(MaxBatchSize))
        {
            DbSet.RemoveRange(chunk);
            Rail<int> saveResult = await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (!saveResult.IsSuccess(out _, out _))
                return saveResult.Error!.Value;
        }

        return entities;
    }

    /// <inheritdoc />
    public virtual async Task<Rail<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            int count = await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Count > 0 ? ex.Entries[0] : null;
            string entityType = entry?.Metadata.Name ?? typeof(TEntity).Name;
            string entityId = GetPrimaryKeyValue(entry);
            return new UnionError.Conflict(
                $"A concurrency conflict was detected for {entityType} with ID {entityId}. The entity was modified by another process."
            );
        }
        catch (DbUpdateException ex)
        {
            return new UnionError.Validation(
                new Dictionary<string, string[]> { ["$"] = [ex.InnerException?.Message ?? ex.Message] }
            );
        }
    }

    private static string GetPrimaryKeyValue(EntityEntry? entry)
    {
        if (entry is null)
            return "Unknown";

        var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());

        return primaryKey?.CurrentValue?.ToString() ?? "Unknown";
    }

    /// <inheritdoc />
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction != null)
            throw new InvalidOperationException("A transaction is already active.");

        _ = await Context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("No transaction is active to commit.");

        await Context.Database.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("No transaction is active to roll back.");

        await Context.Database.RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
    }
}
