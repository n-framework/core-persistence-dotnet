namespace NFramework.Persistence.Abstractions.Entities;

/// <summary>
/// Domain entity base class for soft-deleted entities.
/// Automatically manages deletion status and timestamps.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>
    where TId : IEquatable<TId>
{
    [ThreadStatic]
    private static bool IsSyncing;

    /// <summary>
    /// Boolean flag for fast query filtering.
    /// Setting this to true without a DeletedAt value will set DeletedAt to UTC now.
    /// Setting this to false will clear DeletedAt.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Sonar Analyzer",
        "S2696:Make the enclosing instance property 'static' or remove this set on the 'static' field.",
        Justification = "ThreadStatic guard for recursion is safe"
    )]
    public bool IsDeleted
    {
        get;
        set
        {
            if (IsSyncing)
            {
                field = value;
                return;
            }

            IsSyncing = true;
            try
            {
                field = value;
                if (field && DeletedAt == null)
                {
                    DeletedAt = DateTime.UtcNow;
                }
                else if (!field)
                {
                    DeletedAt = null;
                }
            }
            finally
            {
                IsSyncing = false;
            }
        }
    }

    /// <summary>
    /// Timestamp recording when the entity was soft-deleted.
    /// Setting this to a non-null value will set IsDeleted to true.
    /// Setting this to null will set IsDeleted to false.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Sonar Analyzer",
        "S2696:Make the enclosing instance property 'static' or remove this set on the 'static' field.",
        Justification = "ThreadStatic guard for recursion is safe"
    )]
    public DateTime? DeletedAt
    {
        get;
        set
        {
            if (IsSyncing)
            {
                field = value;
                return;
            }

            IsSyncing = true;
            try
            {
                field = value;
                IsDeleted = field != null;
            }
            finally
            {
                IsSyncing = false;
            }
        }
    }
}
