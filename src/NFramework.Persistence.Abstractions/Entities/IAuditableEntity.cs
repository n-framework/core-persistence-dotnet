namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Contract for entities that require audit tracking.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}
