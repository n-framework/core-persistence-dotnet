namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Base entity with identity and optimistic concurrency support.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class Entity<TId>
    where TId : IEquatable<TId>
{
    public TId Id { get; set; } = default!;

    /// <summary>
    /// Optimistic concurrency token. Automatically managed by the database.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1819:Properties should not return arrays",
        Justification = "EF Core RowVersion requires byte array"
    )]
    public byte[]? RowVersion { get; set; }
}
