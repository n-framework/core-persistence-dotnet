using NFramework.Persistence.Abstractions.Dynamic;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Encapsulates dynamic query parameters to reduce method argument counts.
/// </summary>
public record DynamicQueryOption(
    IReadOnlyCollection<Filter>? Filters = null,
    IReadOnlyCollection<Order>? Orders = null,
    QueryTrackingMode Tracking = QueryTrackingMode.Default
) : IDynamicFilterableQuery, IDynamicOrderableQuery, IQueryTracking;
