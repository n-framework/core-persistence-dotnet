using System.Linq.Expressions;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Defines standard predicate-based filtering for a query.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IFilterableQuery<TEntity>
{
    /// <summary>Filter condition to apply.</summary>
    Expression<Func<TEntity, bool>>? Predicate { get; init; }
}
