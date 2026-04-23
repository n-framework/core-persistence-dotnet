using System.ComponentModel.DataAnnotations;

namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Represents a filter specification for dynamic queries.
/// Supports both simple field comparisons and nested logical groups.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Sonar Analyzer",
    "S2325:Methods and properties that don't access instance data should be 'static'",
    Justification = "False positive with C# 14 field keyword"
)]
public class Filter : IValidatableObject
{
    public Filter() { }

    public Filter(string field, FilterOperator @operator, object? value = null)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field name cannot be empty.", nameof(field));

        Field = field;
        Operator = @operator;
        Value = value;
    }

    public Filter(FilterLogic logic, ICollection<Filter> filters)
    {
        Logic = logic;
        Filters = filters ?? throw new ArgumentNullException(nameof(filters));
    }

    /// <summary>Property name on the entity to filter.</summary>
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

    /// <summary>
    /// Comparison operator.
    /// </summary>
    public FilterOperator Operator { get; set; }

    /// <summary>Value to compare against. Null for null-check operators.</summary>
    public object? Value { get; set; }

    /// <summary>When true, negates the filter condition.</summary>
    public bool IsNot { get; set; }

    /// <summary>When true, string comparisons are case-sensitive.</summary>
    public bool CaseSensitive { get; set; }

    /// <summary>
    /// Logical operator for combining with nested filters.
    /// </summary>
    public FilterLogic? Logic { get; set; }

    /// <summary>Nested filters combined using <see cref="Logic"/>.</summary>
    public ICollection<Filter>? Filters { get; init; }

    /// <summary>
    /// Validates the filter state.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Field) && !Logic.HasValue)
        {
            yield return new ValidationResult(
                "Filter must have either a Field or Logic with nested filters.",
                [nameof(Field), nameof(Logic)]
            );
        }

        if (!Logic.HasValue)
        {
            if (Operator is FilterOperator.IsNull or FilterOperator.IsNotNull)
            {
                if (Value != null)
                {
                    yield return new ValidationResult($"Operator {Operator} does not expect a value.", [nameof(Value)]);
                }
            }
            else if (Value == null)
            {
                yield return new ValidationResult($"Operator {Operator} requires a comparison value.", [nameof(Value)]);
            }
            else if (Operator == FilterOperator.In && Value is not System.Collections.IEnumerable)
            {
                yield return new ValidationResult(
                    $"Operator {Operator} requires an IEnumerable value.",
                    [nameof(Value)]
                );
            }
        }

        if (Logic.HasValue && (Filters == null || Filters.Count == 0))
        {
            yield return new ValidationResult(
                "Logic operator requires at least one nested filter.",
                [nameof(Logic), nameof(Filters)]
            );
        }

        if (!Logic.HasValue && Filters != null && Filters.Count > 0)
        {
            yield return new ValidationResult(
                "Nested filters require a logic operator.",
                [nameof(Logic), nameof(Filters)]
            );
        }

        if (Filters != null)
        {
            foreach (var filter in Filters)
            {
                ValidationContext childContext = new(filter);
                foreach (var result in filter.Validate(childContext))
                {
                    yield return result;
                }
            }
        }
    }
}
