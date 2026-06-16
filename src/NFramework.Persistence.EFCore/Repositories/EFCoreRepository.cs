using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Repositories;
using UnionRailway;

namespace NFramework.Persistence.EFCore.Repositories;

/// <summary>
/// Base EF Core repository implementing all NFramework persistence contracts.
/// <para>
/// Error handling convention:
/// Throws <see cref="ArgumentNullException"/> for programming errors (null required arguments).
/// Returns <see cref="Rail{T}"/> errors for domain/runtime errors (not found, result set exceeded).
/// </para>
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
    /// The default maximum number of results allowed for a single non-paginated query.
    /// </summary>
    public const int DefaultMaxResultSetSize = 10000;

    /// <summary>
    /// The default maximum number of entities allowed in a single bulk operation chunk.
    /// </summary>
    protected const int DefaultMaxBatchSize = 1000;

    /// <summary>
    /// The maximum number of results allowed for a single non-paginated query.
    /// Defaults to <see cref="DefaultMaxResultSetSize"/>.
    /// Set to 0 or null to disable protection (Not Recommended).
    /// </summary>
    protected virtual int? MaxResultSetSize => DefaultMaxResultSetSize;

    /// <summary>
    /// The maximum number of entities processed in a single database roundtrip during bulk operations.
    /// Defaults to <see cref="DefaultMaxBatchSize"/>.
    /// </summary>
    protected virtual int MaxBatchSize => DefaultMaxBatchSize;

    /// <summary>
    /// Enforces the <see cref="MaxResultSetSize"/> limit on a query.
    /// Fetches <c>limit + 1</c> rows to detect overflow without a separate COUNT round-trip.
    /// </summary>
    protected async Task<Rail<IReadOnlyList<T>>> ExecuteWithLimitAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken
    )
    {
        if (MaxResultSetSize is not { } limit || limit <= 0)
            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        var results = await query.Take(limit + 1).ToListAsync(cancellationToken).ConfigureAwait(false);

        return results.Count <= limit
            ? results
            : new UnionError.Custom(
                Code: "RESULT_SET_EXCEEDED",
                Message: $"The result set size exceeded the configured limit of {limit} records. "
                    + "Please use pagination or more restrictive filters."
            );
    }
}
