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

        TEntity? existing = await DbSet.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
        if (existing == null)
            return await AddAsync(entity, cancellationToken).ConfigureAwait(false);

        Context.Entry(existing).CurrentValues.SetValues(entity);
        return existing;
    }

    /// <inheritdoc />
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
        return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
