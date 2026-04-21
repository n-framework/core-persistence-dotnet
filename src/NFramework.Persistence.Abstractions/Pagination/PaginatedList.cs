namespace NFramework.Persistence.Abstractions.Pagination;

/// <summary>
/// Default implementation of a paginated list result.
/// </summary>
/// <typeparam name="T">Type of elements in the list.</typeparam>
public record PaginatedList<T>(IList<T> Items, PagingMeta Meta);
