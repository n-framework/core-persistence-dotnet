namespace NFramework.Persistence.Abstractions.Pagination;

/// <summary>
/// Default implementation of a paginated list result.
/// </summary>
/// <typeparam name="T">Type of elements in the list.</typeparam>
public record PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public PagingMeta Meta { get; init; }

    public PaginatedList(IReadOnlyList<T> items, PagingMeta meta)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count > meta.TotalCount)
        {
            throw new ArgumentException(
                $"Items count ({items.Count}) cannot be greater than TotalCount ({meta.TotalCount}).",
                nameof(items)
            );
        }

        Items = items;
        Meta = meta;
    }
}
