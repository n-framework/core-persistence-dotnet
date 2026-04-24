using System.Linq.Dynamic.Core;
using NFramework.Persistence.Abstractions.Dynamic;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Translates abstraction-layer <see cref="Filter"/> and <see cref="Order"/> descriptors
/// into <see cref="IQueryable{T}"/> operations via System.Linq.Dynamic.Core.
/// </summary>
public static partial class DynamicQueryExtensions
{
    [System.Text.RegularExpressions.GeneratedRegex(
        @"^[a-zA-Z_][a-zA-Z0-9_]*(\.[a-zA-Z_][a-zA-Z0-9_]*)*$",
        System.Text.RegularExpressions.RegexOptions.Compiled
    )]
    private static partial System.Text.RegularExpressions.Regex FieldNameRegex { get; }

    extension<T>(IQueryable<T> source)
        where T : class
    {
        /// <summary>
        /// Applies a collection of <see cref="Filter"/> descriptors to the query.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
            "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
        )]
        public IQueryable<T> ApplyFilters(IReadOnlyCollection<Filter>? filters)
        {
            if (filters == null || filters.Count == 0)
                return source;

            IQueryable<T> result = source;
            foreach (Filter filter in filters)
                result = result.ApplySingleFilter(filter);

            return result;
        }

        /// <summary>
        /// Applies a collection of <see cref="Order"/> descriptors to the query.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
            "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
        )]
        public IQueryable<T> ApplyOrders(IReadOnlyCollection<Order>? orders)
        {
            if (orders == null || orders.Count == 0)
                return source;

            var invalidOrder = orders.FirstOrDefault(o => !IsValidField(source, o.Field));
            if (invalidOrder != null)
                throw new ArgumentException($"Invalid or unsafe order field name: '{invalidOrder.Field}'");

            IEnumerable<string> orderClauses = orders.Select(o =>
                $"{o.Field} {(o.Direction == OrderDirection.Desc ? "desc" : "asc")}"
            );

            return source.OrderBy(string.Join(", ", orderClauses));
        }
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    private static IQueryable<T> ApplySingleFilter<T>(this IQueryable<T> source, Filter filter)
        where T : class
    {
        if (filter.Logic.HasValue && filter.Filters is { Count: > 0 })
            return ApplyLogicGroup(source, filter);

        (string expression, object?[] args) = BuildFilterExpression(source, filter);
        return source.Where(expression, args);
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    private static IQueryable<T> ApplyLogicGroup<T>(IQueryable<T> source, Filter group)
        where T : class
    {
        List<string> parts = [];
        List<object?> allArgs = [];
        int paramIndex = 0;

        foreach (Filter childFilter in group.Filters!)
        {
            (string expr, object?[] args) = BuildFilterExpression(source, childFilter, paramIndex);
            parts.Add($"({expr})");
            allArgs.AddRange(args);
            paramIndex += args.Length;
        }

        string connector = group.Logic == FilterLogic.Or ? " || " : " && ";
        string combined = string.Join(connector, parts);

        return source.Where(combined, [.. allArgs]);
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Dynamic query translation uses reflection-based System.Linq.Dynamic.Core which is not fully trim-safe."
    )]
    private static (string Expression, object?[] Args) BuildFilterExpression<T>(
        IQueryable<T> source,
        Filter filter,
        int paramOffset = 0
    )
        where T : class
    {
        var results = filter.Validate(null!);
        foreach (var result in results)
        {
            if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                throw new System.ComponentModel.DataAnnotations.ValidationException(result, null, filter);
        }

        string fieldName = filter.Field;
        if (!IsValidField(source, fieldName))
            throw new ArgumentException($"Invalid or unsafe field name: '{fieldName}'");

        string paramName = $"@{paramOffset}";

        (string expr, object?[] args) = filter.Operator switch
        {
            FilterOperator.Equal => ($"{fieldName} == {paramName}", [filter.Value]),
            FilterOperator.NotEqual => ($"{fieldName} != {paramName}", [filter.Value]),
            FilterOperator.LessThan => ($"{fieldName} < {paramName}", [filter.Value]),
            FilterOperator.LessThanOrEqual => ($"{fieldName} <= {paramName}", [filter.Value]),
            FilterOperator.GreaterThan => ($"{fieldName} > {paramName}", [filter.Value]),
            FilterOperator.GreaterThanOrEqual => ($"{fieldName} >= {paramName}", [filter.Value]),
            FilterOperator.IsNull => ($"{fieldName} == null", []),
            FilterOperator.IsNotNull => ($"{fieldName} != null", []),
            FilterOperator.StartsWith => filter.CaseSensitive
                ? ($"{fieldName}.StartsWith({paramName})", [filter.Value])
                : ($"{fieldName}.ToUpper().StartsWith({paramName})", [filter.Value?.ToString()?.ToUpperInvariant()]),
            FilterOperator.EndsWith => filter.CaseSensitive
                ? ($"{fieldName}.EndsWith({paramName})", [filter.Value])
                : ($"{fieldName}.ToUpper().EndsWith({paramName})", [filter.Value?.ToString()?.ToUpperInvariant()]),
            FilterOperator.Contains => filter.CaseSensitive
                ? ($"{fieldName}.Contains({paramName})", [filter.Value])
                : ($"{fieldName}.ToUpper().Contains({paramName})", [filter.Value?.ToString()?.ToUpperInvariant()]),
            FilterOperator.DoesNotContain => filter.CaseSensitive
                ? ($"!{fieldName}.Contains({paramName})", [filter.Value])
                : ($"!{fieldName}.ToUpper().Contains({paramName})", [filter.Value?.ToString()?.ToUpperInvariant()]),
            FilterOperator.In => ($"{paramName}.Contains({fieldName})", (object?[])[filter.Value]),
            _ => throw new NotSupportedException(
                $"The filter operator '{filter.Operator}' is not supported for field '{fieldName}'."
            ),
        };

        if (filter.IsNot)
            expr = $"!({expr})";

        return (expr, args);
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Dynamic query validation uses reflection or EF Core metadata which is not fully trim-safe."
    )]
    private static bool IsValidField<T>(IQueryable<T> source, string? fieldName)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(fieldName) || !FieldNameRegex.IsMatch(fieldName))
            return false;

        if (source is Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<IServiceProvider> infrastructure)
        {
            var model = (Microsoft.EntityFrameworkCore.Metadata.IModel?)
                infrastructure.Instance.GetService(typeof(Microsoft.EntityFrameworkCore.Metadata.IModel));

            if (model != null)
            {
                var entityType = model.FindEntityType(typeof(T));
                if (entityType != null)
                {
                    string[] parts = fieldName.Split('.');
                    Microsoft.EntityFrameworkCore.Metadata.IReadOnlyEntityType? currentType = entityType;

                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (currentType == null)
                            return false;

                        string part = parts[i];

                        var property = currentType.FindProperty(part);
                        var navigation = currentType.FindNavigation(part);

                        if (property == null && navigation == null)
                            return false;

                        if (i < parts.Length - 1)
                        {
                            if (navigation == null)
                                return false;

                            currentType = navigation.TargetEntityType;
                        }
                    }

                    return true;
                }
            }
        }

        // Fallback for non-EF Core queryables (e.g., in-memory mocks using Array.AsQueryable)
        Type currentReflectionType = typeof(T);
        foreach (string part in fieldName.Split('.'))
        {
            System.Reflection.PropertyInfo? propInfo = currentReflectionType.GetProperty(
                part,
                System.Reflection.BindingFlags.IgnoreCase
                    | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
            );

            if (propInfo == null)
                return false;

            currentReflectionType = propInfo.PropertyType;

            if (
                currentReflectionType != typeof(string)
                && typeof(System.Collections.IEnumerable).IsAssignableFrom(currentReflectionType)
            )
            {
                Type? elementType = currentReflectionType.IsGenericType
                    ? currentReflectionType.GetGenericArguments().FirstOrDefault()
                    : currentReflectionType.GetElementType();

                if (elementType != null)
                {
                    currentReflectionType = elementType;
                }
            }
        }

        return true;
    }
}
