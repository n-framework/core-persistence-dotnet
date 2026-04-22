using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Constants;
using NFramework.Persistence.EFCore.Extensions;

namespace NFramework.Persistence.EFCore.Repositories;

public abstract partial class EFCoreRepository<TEntity, TId, TContext>
{
    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = DbSet;
        if (predicate != null)
            query = query.Where(predicate);

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(
        QueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = buildQuery(options);
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<PaginatedList<TEntity>> GetListAsync(
        PageableQueryOption<TEntity>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = buildQuery(options);
        Paging paging = options?.Page ?? Paging.Default;
        return await query.ToPaginatedListAsync(paging, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return predicate != null
            ? await DbSet.AnyAsync(predicate, cancellationToken).ConfigureAwait(false)
            : await DbSet.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return predicate != null
            ? await DbSet.CountAsync(predicate, cancellationToken).ConfigureAwait(false)
            : await DbSet.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    private IQueryable<TEntity> buildQuery(QueryOption<TEntity>? options)
    {
        IQueryable<TEntity> query = DbSet;

        if (options is IQueryOptionWithSoftDelete { IncludeDeleted: true })
            query = query.IgnoreQueryFilters(QueryFilters.SoftDeletionArray);

        if (options?.Predicate != null)
            query = query.Where(options.Predicate);

        if (options?.OrderBy != null)
            query = options.OrderBy(query);

        return query;
    }
}
