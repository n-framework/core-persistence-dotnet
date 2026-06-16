using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Extensions;
using UnionRailway;

namespace NFramework.Persistence.EFCore.Repositories;

public abstract partial class EFCoreRepository<TEntity, TId, TContext>
{
    /// <inheritdoc />
    public virtual async Task<Rail<TResult>> GetSelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        QueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(selector);

        IQueryable<TEntity> query = buildQuery(options);
        TResult? result = await query.Select(selector).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return result is not null ? result : new UnionError.NotFound(typeof(TEntity).Name);
    }

    /// <inheritdoc />
    public virtual async Task<Rail<IReadOnlyList<TResult>>> GetAllSelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        QueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        IQueryable<TEntity> query = buildQuery(options);
        IQueryable<TResult> projected = query.Select(selector);
        return await ExecuteWithLimitAsync(projected, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<Rail<PaginatedList<TResult>>> GetListSelectAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        PageableQueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(selector);

        IQueryable<TEntity> query = buildQuery(options);
        IQueryable<TResult> projected = query.Select(selector);
        Paging paging = options?.Page ?? Paging.Default;
        return await projected.ToPaginatedListAsync(paging, cancellationToken).ConfigureAwait(false);
    }
}
