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
    public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = DbSet.Update(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            TEntity? existing = await DbSet.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
            if (existing == null)
                return await AddAsync(entity, cancellationToken).ConfigureAwait(false);

            byte[] callerRowVersion = entity.RowVersion;
            Context.Entry(existing).CurrentValues.SetValues(entity);
            Context.Entry(existing).Property(e => e.RowVersion).OriginalValue = callerRowVersion;
            return existing;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(
                $"A concurrency conflict was detected during upsert for {typeof(TEntity).Name} with ID {entity.Id}.",
                ex
            );
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// For entities inheriting from SoftDeletableEntity, this operation will be translated
    /// into a soft delete by the AuditSaveChangesInterceptor during SaveChangesAsync.
    /// </remarks>
    public virtual Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = DbSet.Remove(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual async Task<int> BulkAddAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        List<TEntity> list = [.. entities];
        if (list.Count == 0)
            return 0;

        await DbSet.AddRangeAsync(list, cancellationToken).ConfigureAwait(false);
        return list.Count;
    }

    /// <inheritdoc />
    public virtual Task<int> BulkUpdateAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        List<TEntity> list = [.. entities];
        if (list.Count == 0)
            return Task.FromResult(0);

        DbSet.UpdateRange(list);
        return Task.FromResult(list.Count);
    }

    /// <inheritdoc />
    public virtual Task<int> BulkDeleteAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        List<TEntity> list = [.. entities];
        if (list.Count == 0)
            return Task.FromResult(0);

        DbSet.RemoveRange(list);
        return Task.FromResult(list.Count);
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
            throw new ConcurrencyConflictException(
                $"A concurrency conflict was detected for {entityType} with ID {entityId}. The entity was modified by another process.",
                ex
            );
        }
    }

    /// <inheritdoc />
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction != null)
            return;

        _ = await Context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction == null)
            return;

        await Context.Database.CurrentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Context.Database.CurrentTransaction == null)
            return;

        await Context.Database.CurrentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
    }
}
