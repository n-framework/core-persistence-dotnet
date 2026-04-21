namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Entity supporting soft delete with both a boolean flag (fast queries)
/// and a deletion timestamp (audit trail).
/// Use this for business entities where deleted data should be recoverable.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Boolean flag for fast query filtering. Indexed for performance.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp recording when the entity was soft-deleted. Used for audit trails.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
