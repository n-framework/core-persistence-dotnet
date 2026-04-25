namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Defines dynamic runtime ordering behavior for a query.
/// </summary>
public interface IDynamicOrderableQuery
{
    /// <summary>Order specifications.</summary>
    IReadOnlyCollection<Order>? Orders { get; init; }
}
