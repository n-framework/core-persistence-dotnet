using System.Linq.Dynamic.Core;
using NFramework.Persistence.Abstractions.Dynamic;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Translates abstraction-layer <see cref="Filter"/> and <see cref="Order"/> descriptors
/// into <see cref="IQueryable{T}"/> operations via System.Linq.Dynamic.Core.
/// </summary>
public static class DynamicQueryExtensions
{
    extension<T>(IQueryable<T> source)
        where T : class
    {
        /// <summary>
        /// Applies a collection of <see cref="Filter"/> descriptors to the query.
        /// </summary>
        public IQueryable<T> ApplyFilters(ICollection<Filter>? filters)
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
        public IQueryable<T> ApplyOrders(ICollection<Order>? orders)
        {
            if (orders == null || orders.Count == 0)
                return source;

            IEnumerable<string> orderClauses = orders.Select(o =>
                $"{o.Field} {(o.Direction == OrderDirection.Desc ? "desc" : "asc")}"
            );

            try
            {
                return source.OrderBy(string.Join(", ", orderClauses));
            }
            catch (System.Linq.Dynamic.Core.Exceptions.ParseException ex)
            {
                throw new ArgumentException(
                    $"Invalid order field '{string.Join(", ", orders.Select(o => o.Field))}': {ex.Message}",
                    ex
                );
            }
        }
    }

    private static IQueryable<T> ApplySingleFilter<T>(this IQueryable<T> source, Filter filter)
        where T : class
    {
        if (filter.Logic.HasValue && filter.Filters is { Count: > 0 })
            return ApplyLogicGroup(source, filter);

        (string expression, object?[] args) = BuildFilterExpression(filter);
        try
        {
            return source.Where(expression, args);
        }
        catch (System.Linq.Dynamic.Core.Exceptions.ParseException ex)
        {
            throw new ArgumentException($"Invalid filter field '{filter.Field}': {ex.Message}", ex);
        }
    }

    private static IQueryable<T> ApplyLogicGroup<T>(IQueryable<T> source, Filter group)
        where T : class
    {
        List<string> parts = [];
        List<object?> allArgs = [];
        int paramIndex = 0;

        foreach (Filter childFilter in group.Filters!)
        {
            (string expr, object?[] args) = BuildFilterExpression(childFilter, paramIndex);
            parts.Add($"({expr})");
            allArgs.AddRange(args);
            paramIndex += args.Length;
        }

        string connector = group.Logic == FilterLogic.Or ? " || " : " && ";
        string combined = string.Join(connector, parts);

        try
        {
            return source.Where(combined, [.. allArgs]);
        }
        catch (System.Linq.Dynamic.Core.Exceptions.ParseException ex)
        {
            throw new ArgumentException($"Invalid filter group expression: {ex.Message}", ex);
        }
    }

    private static (string Expression, object?[] Args) BuildFilterExpression(Filter filter, int paramOffset = 0)
    {
        System.ComponentModel.DataAnnotations.Validator.ValidateObject(
            filter,
            new System.ComponentModel.DataAnnotations.ValidationContext(filter),
            true
        );

        string fieldName = filter.Field;
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
        {
            expr = $"!({expr})";
        }

        return (expr, args);
    }
}
