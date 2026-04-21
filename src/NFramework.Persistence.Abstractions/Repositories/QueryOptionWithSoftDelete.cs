using System.Linq.Expressions;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Query options with soft delete support.
/// </summary>
public record QueryOptionWithSoftDelete<TEntity>(
    Expression<Func<TEntity, bool>>? Predicate = null,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy = null,
    bool IncludeDeleted = false
) : QueryOption<TEntity>(Predicate, OrderBy), IQueryOptionWithSoftDelete;
