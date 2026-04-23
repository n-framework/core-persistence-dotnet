namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Domain entity base class for soft-deleted entities.
/// Automatically manages deletion status and timestamps.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>, ISoftDeletableEntity
    where TId : IEquatable<TId>
{
    private bool _isSyncing;

    /// <summary>
    /// Boolean flag for fast query filtering.
    /// Setting this to true without a DeletedAt value will set DeletedAt to UTC now.
    /// Setting this to false will clear DeletedAt.
    /// </summary>
    public bool IsDeleted
    {
        get;
        set
        {
            if (_isSyncing)
            {
                field = value;
                return;
            }

            _isSyncing = true;
            try
            {
                field = value;
                if (field && DeletedAt == null)
                    DeletedAt = DateTime.UtcNow;
                else if (!field)
                    DeletedAt = null;
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }

    /// <summary>
    /// Timestamp recording when the entity was soft-deleted.
    /// Setting this to a non-null value will set IsDeleted to true.
    /// Setting this to null will set IsDeleted to false.
    /// </summary>
    public DateTime? DeletedAt
    {
        get;
        set
        {
            if (_isSyncing)
            {
                field = value;
                return;
            }

            _isSyncing = true;
            try
            {
                field = value;
                IsDeleted = field != null;
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
