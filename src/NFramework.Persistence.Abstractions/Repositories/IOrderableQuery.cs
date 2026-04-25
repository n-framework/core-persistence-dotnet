namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Defines ordering behavior for a query.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IOrderableQuery<TEntity>
{
    /// <summary>Order specification.</summary>
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; init; }
}
