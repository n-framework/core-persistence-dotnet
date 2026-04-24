namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Entity with automatic timestamp tracking.
/// Use this for entities that need creation and modification audit trails.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
    where TId : IEquatable<TId>
{
    protected AuditableEntity(TId id)
        : base(id) { }

    protected AuditableEntity()
        : base() { }

    /// <summary>
    /// Timestamp set when the entity is first persisted.
    /// Implementations are responsible for setting this value.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp updated on every modification.
    /// Implementations are responsible for maintaining this value.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Validates the entity state.
    /// </summary>
    public virtual IEnumerable<string> Validate()
    {
        if (CreatedAt == default)
            yield return "CreatedAt must be a valid timestamp.";

        if (UpdatedAt.HasValue && UpdatedAt.Value < CreatedAt)
            yield return "UpdatedAt cannot be earlier than CreatedAt.";
    }
}
