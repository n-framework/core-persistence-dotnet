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
    /// Timestamp set when the entity is first persisted.
    /// Implementations are responsible for setting this value.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp updated on every modification.
    /// Implementations are responsible for maintaining this value.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if value is earlier than CreatedAt.</exception>
    public DateTime UpdatedAt
    {
        get;
        set
        {
            if (value < CreatedAt)
                throw new ArgumentException("UpdatedAt cannot be earlier than CreatedAt.");

            field = value;
        }
    }
}
