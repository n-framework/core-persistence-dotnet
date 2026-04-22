using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Constants;
using NFramework.Persistence.EFCore.Extensions;

namespace NFramework.Persistence.EFCore.Repositories;

public abstract partial class EFCoreRepository<TEntity, TId, TContext>
{
    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<PaginatedList<TEntity>> GetListByDynamicAsync(
        PageableDynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await query.ToPaginatedListAsync(options.Page, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    private IQueryable<TEntity> buildDynamicQuery(DynamicQueryOption options)
    {
        IQueryable<TEntity> query = DbSet;

        if (options is IQueryOptionWithSoftDelete { IncludeDeleted: true })
            query = query.IgnoreQueryFilters(QueryFilters.SoftDeletionArray);

        query = query.ApplyFilters(options.Filters);
        query = query.ApplyOrders(options.Orders);
        return query;
    }
}
