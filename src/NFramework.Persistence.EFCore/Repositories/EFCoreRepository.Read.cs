using System.Linq.Expressions;
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
    public virtual async Task<Rail<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        TEntity? entity = await DbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);
        return entity is not null ? entity : new UnionError.NotFound(typeof(TEntity).Name);
    }

    /// <inheritdoc />
    public virtual async Task<Rail<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = DbSet;
        if (predicate != null)
            query = query.Where(predicate);

        TEntity? entity = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return entity is not null ? entity : new UnionError.NotFound(typeof(TEntity).Name);
    }

    /// <inheritdoc />
    public virtual async Task<Rail<IReadOnlyList<TEntity>>> GetAllAsync(
        QueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = BuildQuery(options);
        return await ExecuteWithLimitAsync(query, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<Rail<PaginatedList<TEntity>>> GetListAsync(
        PageableQueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = BuildQuery(options);
        Paging paging = options?.Page ?? Paging.Default;
        return await query.ToPaginatedListAsync(paging, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<Rail<bool>> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    ) =>
        predicate != null
            ? await DbSet.AnyAsync(predicate, cancellationToken).ConfigureAwait(false)
            : await DbSet.AnyAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public virtual async Task<Rail<int>> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    ) =>
        predicate != null
            ? await DbSet.CountAsync(predicate, cancellationToken).ConfigureAwait(false)
            : await DbSet.CountAsync(cancellationToken).ConfigureAwait(false);

    private IQueryable<TEntity> BuildQuery(QueryOption<TEntity>? options)
    {
        IQueryable<TEntity> query = DbSet;

        if (options is IQueryOptionWithSoftDelete { IncludeDeleted: true })
            query = query.IgnoreQueryFilters(QueryFilters.SoftDeletionArray);

        query = query.ApplyTracking(options);

        if (options?.Predicate != null)
            query = query.Where(options.Predicate);

        if (options?.OrderBy != null)
            query = options.OrderBy(query);

        return query;
    }
}
