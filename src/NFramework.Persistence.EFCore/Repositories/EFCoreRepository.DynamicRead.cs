using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Constants;
using NFramework.Persistence.EFCore.Extensions;
using UnionRailway;

namespace NFramework.Persistence.EFCore.Repositories;

public abstract partial class EFCoreRepository<TEntity, TId, TContext>
{
    /// <inheritdoc />
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    public virtual async Task<Rail<TEntity>> GetByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        TEntity? entity = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return entity is not null ? entity : new UnionError.NotFound(typeof(TEntity).Name);
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    public virtual async Task<Rail<IReadOnlyList<TEntity>>> GetAllByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await ExecuteWithLimitAsync(query, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    public virtual async Task<Rail<PaginatedList<TEntity>>> GetListByDynamicAsync(
        PageableDynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await query.ToPaginatedListAsync(options.Page, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    public virtual async Task<Rail<bool>> AnyByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    public virtual async Task<Rail<int>> CountByDynamicAsync(
        DynamicQueryOption options,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        IQueryable<TEntity> query = buildDynamicQuery(options);
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    [RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    private IQueryable<TEntity> buildDynamicQuery(DynamicQueryOption options)
    {
        IQueryable<TEntity> query = DbSet;

        if (options is IQueryOptionWithSoftDelete { IncludeDeleted: true })
            query = query.IgnoreQueryFilters(QueryFilters.SoftDeletionArray);

        query = query.ApplyTracking(options);
        query = query.ApplySplitting(options);
        query = query.ApplyFilters(options.Filters);
        query = query.ApplyOrders(options.Orders);
        return query;
    }
}
