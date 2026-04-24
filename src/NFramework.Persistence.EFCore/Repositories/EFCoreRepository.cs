using System.Diagnostics.CodeAnalysis;
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
public abstract partial class EFCoreRepository<
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.NonPublicProperties
            | DynamicallyAccessedMemberTypes.Interfaces
    )]
        TEntity,
    TId,
    TContext
>(TContext context)
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

    /// <summary>
    /// The maximum number of results allowed for a single non-paginated query.
    /// Defaults to 10,000. Set to 0 or null to disable protection (Not Recommended).
    /// </summary>
    protected virtual int? MaxResultSetSize => 10000;

    /// <summary>
    /// Enforces the <see cref="MaxResultSetSize"/> limit on a query.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the query results exceed the limit.</exception>
    protected async Task<IReadOnlyList<TEntity>> ExecuteWithLimitAsync(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken
    )
    {
        if (MaxResultSetSize is not { } limit || limit <= 0)
            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        // We take limit + 1 to check if there are more records than allowed
        var results = await query.Take(limit + 1).ToListAsync(cancellationToken).ConfigureAwait(false);

        return results.Count <= limit
            ? results
            : throw new InvalidOperationException(
                $"The result set size exceeded the configured limit of {limit} records. "
                    + "Please use pagination or more restrictive filters."
            );
    }
}
