namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Contract for entities that support soft deletion.
/// </summary>
public interface ISoftDeletableEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
