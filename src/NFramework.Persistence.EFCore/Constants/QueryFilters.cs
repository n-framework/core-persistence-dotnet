namespace NFramework.Persistence.EFCore.Constants;

/// <summary>
/// Provides constant names for EF Core global query filters.
/// </summary>
public static class QueryFilters
{
    /// <summary>
    /// The name of the soft-deletion query filter.
    /// </summary>
    public const string SoftDeletion = "NFW_SoftDeletionFilter";

    /// <summary>
    /// Reusable array containing the soft-deletion filter name to avoid per-query allocations.
    /// </summary>
    public static readonly string[] SoftDeletionArray = [SoftDeletion];
}
