using System.ComponentModel.DataAnnotations;

namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Entity with automatic timestamp tracking.
/// Use this for entities that need creation and modification audit trails.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity, IValidatableObject
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

    /// <inheritdoc />
    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreatedAt == default)
            yield return new ValidationResult("CreatedAt must be a valid timestamp.", [nameof(CreatedAt)]);

        if (UpdatedAt.HasValue && UpdatedAt.Value < CreatedAt)
            yield return new ValidationResult("UpdatedAt cannot be earlier than CreatedAt.", [nameof(UpdatedAt)]);
    }
}
