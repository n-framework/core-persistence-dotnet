using NFramework.Persistence.Abstractions.Pagination;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Encapsulates paginated dynamic query parameters.
/// </summary>
public record PageableDynamicQueryOption : DynamicQueryOption, IPageableQuery
{
    /// <summary>
    /// Pagination request parameters.
    /// </summary>
    public Paging Page { get; init; } = new(0, 10);
}
