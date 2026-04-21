using System.Linq.Dynamic.Core;
using NFramework.Persistence.Abstractions.Dynamic;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Translates abstraction-layer <see cref="Filter"/> and <see cref="Order"/> descriptors
/// into <see cref="IQueryable{T}"/> operations via System.Linq.Dynamic.Core.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Sonar Analyzer",
    "S2325:Methods and properties that don't access instance data should be 'static'",
    Justification = "C# 14 extension block false positive"
)]
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
            {
                return source;
            }

            IQueryable<T> result = source;
            foreach (Filter filter in filters)
            {
                result = result.ApplySingleFilter(filter);
            }

            return result;
        }

        /// <summary>
        /// Applies a collection of <see cref="Order"/> descriptors to the query.
        /// </summary>
        public IQueryable<T> ApplyOrders(ICollection<Order>? orders)
        {
            if (orders == null || orders.Count == 0)
            {
                return source;
            }

            IEnumerable<string> orderClauses = orders.Select(o =>
                $"{o.Field} {(o.Direction == OrderDirection.Desc ? "desc" : "asc")}"
            );

            return source.OrderBy(string.Join(", ", orderClauses));
        }
    }

    private static IQueryable<T> ApplySingleFilter<T>(this IQueryable<T> source, Filter filter)
        where T : class
    {
        if (filter.Logic.HasValue && filter.Filters is { Count: > 0 })
        {
            return ApplyLogicGroup(source, filter);
        }

        (string expression, object?[] args) = BuildFilterExpression(filter);
        return source.Where(expression, args);
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

        return source.Where(combined, [.. allArgs]);
    }

    private static (string Expression, object?[] Args) BuildFilterExpression(Filter filter, int paramOffset = 0)
    {
        string fieldName = filter.Field;
        string paramName = $"@{paramOffset}";

        return filter.Operator switch
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
                : ($"{fieldName}.ToLower().StartsWith({paramName}.ToString().ToLower())", [filter.Value]),
            FilterOperator.EndsWith => filter.CaseSensitive
                ? ($"{fieldName}.EndsWith({paramName})", [filter.Value])
                : ($"{fieldName}.ToLower().EndsWith({paramName}.ToString().ToLower())", [filter.Value]),
            FilterOperator.Contains => filter.CaseSensitive
                ? ($"{fieldName}.Contains({paramName})", [filter.Value])
                : ($"{fieldName}.ToLower().Contains({paramName}.ToString().ToLower())", [filter.Value]),
            FilterOperator.DoesNotContain => filter.CaseSensitive
                ? ($"!{fieldName}.Contains({paramName})", [filter.Value])
                : ($"!{fieldName}.ToLower().Contains({paramName}.ToString().ToLower())", [filter.Value]),
            FilterOperator.In => ($"{paramName}.Contains({fieldName})", [filter.Value]),
            _ => throw new InvalidOperationException($"Unknown filter operator: {filter.Operator}"),
        };
    }
}
