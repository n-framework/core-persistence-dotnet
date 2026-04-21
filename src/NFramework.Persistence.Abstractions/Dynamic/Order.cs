namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Represents an order specification for dynamic queries.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration.", Justification = "False positive with C# 14 field keyword")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Analyzer", "S2325:Methods and properties that don't access instance data should be 'static'", Justification = "False positive with C# 14 field keyword")]
public class Order
{
    /// <summary>Property name to order by.</summary>
    /// <exception cref="ArgumentException">Thrown if value is null or empty.</exception>
    public string Field
    {
        get;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Field name cannot be empty.", nameof(value));
            }

            field = value;
        }
    } = string.Empty;

    /// <summary>Order direction.</summary>
    public OrderDirection Direction { get; set; } = OrderDirection.Asc;
}
