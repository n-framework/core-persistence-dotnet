using System.Linq.Expressions;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Encapsulates standard query parameters to reduce method argument counts.
/// </summary>
public record QueryOption<TEntity>(
    Expression<Func<TEntity, bool>>? Predicate = null,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy = null,
    QueryTrackingMode Tracking = QueryTrackingMode.Default
) : IFilterableQuery<TEntity>, IOrderableQuery<TEntity>, IQueryTracking;
