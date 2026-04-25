using NFramework.Persistence.Abstractions.Entities;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Synchronous read-only query contract.
/// </summary>
public interface IQueryRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets the queryable source.
    /// </summary>
    IQueryable<TEntity> Query();
}
