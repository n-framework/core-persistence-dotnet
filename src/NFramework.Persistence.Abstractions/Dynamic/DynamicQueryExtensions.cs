using System.Linq.Expressions;

namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Provides extension methods for creating strongly-typed dynamic queries.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Sonar Analyzer",
    "S2325:Methods and properties that don't access instance data should be 'static'",
    Justification = "Modern extension block syntax false positive"
)]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1708:Identifiers should differ by more than case",
    Justification = "Experimental extension blocks share the same internal name 'extension'"
)]
public static class DynamicQueryExtensions
{
    extension(Filter filter)
    {
        /// <summary>
        /// Sets the field name using a strongly-typed expression.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="propertyExpression">Expression selecting the property (e.g., u => u.Name).</param>
        /// <returns>The updated filter.</returns>
        public Filter For<T>(Expression<Func<T, object?>> propertyExpression)
        {
            ArgumentNullException.ThrowIfNull(propertyExpression);

            filter.Field = GetPropertyName(propertyExpression);
            return filter;
        }
    }

    extension(Order order)
    {
        /// <summary>
        /// Sets the field name using a strongly-typed expression.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="propertyExpression">Expression selecting the property (e.g., u => u.CreatedAt).</param>
        /// <returns>The updated order.</returns>
        public Order For<T>(Expression<Func<T, object?>> propertyExpression)
        {
            ArgumentNullException.ThrowIfNull(propertyExpression);

            order.Field = GetPropertyName(propertyExpression);
            return order;
        }
    }

    internal static string GetPropertyName<T>(Expression<Func<T, object?>> expression)
    {
        var body = expression.Body;

        // Handle boxing for value types
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
        {
            body = unaryExpression.Operand;
        }

        return body is MemberExpression memberExpression
            ? memberExpression.Member.Name
            : throw new ArgumentException(
                "Expression must be a member expression representing a property.",
                nameof(expression)
            );
    }
}
