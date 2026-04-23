using NFramework.Persistence.Abstractions.Dynamic;

namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Dynamic query options with soft delete support.
/// </summary>
public record DynamicQueryOptionWithSoftDelete(
    IReadOnlyCollection<Filter>? Filters = null,
    IReadOnlyCollection<Order>? Orders = null,
    bool IncludeDeleted = false
) : DynamicQueryOption(Filters, Orders), IQueryOptionWithSoftDelete;
