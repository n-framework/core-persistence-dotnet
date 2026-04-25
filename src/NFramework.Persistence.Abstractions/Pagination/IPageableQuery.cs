namespace NFramework.Persistence.Abstractions.Pagination;

/// <summary>
/// Defines pagination behavior for a query.
/// </summary>
public interface IPageableQuery
{
    /// <summary>Pagination request parameters.</summary>
    Paging Page { get; init; }
}
