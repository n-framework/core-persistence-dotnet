using NFramework.Persistence.Abstractions.Dynamic;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Encapsulates dynamic query parameters to reduce method argument counts.
/// </summary>
public record DynamicQueryOption(ICollection<Filter>? Filters = null, ICollection<Order>? Orders = null)
    : IDynamicFilterableQuery,
        IDynamicOrderableQuery;
