namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Entity with automatic timestamp tracking.
/// Use this for entities that need creation and modification audit trails.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Timestamp set automatically when the entity is first persisted.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp updated automatically on every update.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
