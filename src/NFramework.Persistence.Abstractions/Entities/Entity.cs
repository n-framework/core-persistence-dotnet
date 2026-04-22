namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Base entity with identity and optimistic concurrency support.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class Entity<TId>
    where TId : IEquatable<TId>
{
    public TId Id
    {
        get;
        init
        {
            if (EqualityComparer<TId>.Default.Equals(value, default))
                throw new ArgumentException("Entity ID cannot be the default value.", nameof(value));
            field = value;
        }
    } = default!;

    /// <summary>
    /// Optimistic concurrency token. Automatically managed by the database.
    /// </summary>
    /// <remarks>
    /// This property is used to handle optimistic concurrency conflicts.
    /// If the value in the database differs from the value being saved,
    /// it indicates that another process has modified the record, and a concurrency
    /// exception (typically <c>DbUpdateConcurrencyException</c>) should be thrown.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "EF Core RowVersion requires byte array"
    )]
    public byte[] RowVersion { get; set; } = [];
}
