namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Domain entity base class for soft-deleted entities.
/// Automatically manages deletion status and timestamps.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>, ISoftDeletableEntity
    where TId : IEquatable<TId>
{
    protected SoftDeletableEntity(TId id)
        : base(id) { }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "S1133:Do not forget to remove this deprecated code someday.",
        Justification = "Necessary for ORM parameterless constructor requirement."
    )]
    [Obsolete("Use constructor with ID instead. This is only for ORM use.")]
    protected SoftDeletableEntity()
        : base() { }

    private int _isSyncing;

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
            if (System.Threading.Interlocked.CompareExchange(ref _isSyncing, 1, 0) == 1)
            {
                field = value;
                return;
            }

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
                System.Threading.Volatile.Write(ref _isSyncing, 0);
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
            if (System.Threading.Interlocked.CompareExchange(ref _isSyncing, 1, 0) == 1)
            {
                field = value;
                return;
            }

            try
            {
                field = value;
                IsDeleted = field != null;
            }
            finally
            {
                System.Threading.Volatile.Write(ref _isSyncing, 0);
            }
        }
    }
}
