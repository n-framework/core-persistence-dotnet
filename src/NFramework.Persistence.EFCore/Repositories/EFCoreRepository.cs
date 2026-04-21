using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Repositories;

namespace NFramework.Persistence.EFCore.Repositories;

/// <summary>
/// Base EF Core repository implementing all NFramework persistence contracts.
/// </summary>
/// <typeparam name="TEntity">Entity type inheriting from <see cref="Entity{TId}"/>.</typeparam>
/// <typeparam name="TId">Primary key type implementing <see cref="IEquatable{TId}"/>.</typeparam>
/// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
public abstract partial class EFCoreRepository<TEntity, TId, TContext>(TContext context)
    : IReadRepository<TEntity, TId>,
        IWriteRepository<TEntity, TId>,
        IDynamicReadRepository<TEntity, TId>,
        IQueryRepository<TEntity, TId>,
        IUnitOfWork
    where TEntity : Entity<TId>
    where TId : IEquatable<TId>
    where TContext : DbContext
{
    /// <summary>
    /// The underlying <see cref="DbContext"/>.
    /// </summary>
    protected TContext Context { get; } = context;

    /// <summary>
    /// The <see cref="DbSet{TEntity}"/> for this repository.
    /// </summary>
    protected DbSet<TEntity> DbSet => Context.Set<TEntity>();
}
