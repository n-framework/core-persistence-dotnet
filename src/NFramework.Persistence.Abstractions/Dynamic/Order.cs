namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Represents an order specification for dynamic queries.
/// </summary>
public class Order
{
    /// <summary>Property name to order by.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Order direction.</summary>
    public OrderDirection Direction { get; set; } = OrderDirection.Asc;
}
