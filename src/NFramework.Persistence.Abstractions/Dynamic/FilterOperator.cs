namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Specifies the comparison operator for a dynamic query filter.
/// </summary>
public enum FilterOperator
{
    /// <summary>Equal to.</summary>
    Equal,

    /// <summary>Not equal to.</summary>
    NotEqual,

    /// <summary>Less than.</summary>
    LessThan,

    /// <summary>Less than or equal to.</summary>
    LessThanOrEqual,

    /// <summary>Greater than.</summary>
    GreaterThan,

    /// <summary>Greater than or equal to.</summary>
    GreaterThanOrEqual,

    /// <summary>Is null.</summary>
    IsNull,

    /// <summary>Is not null.</summary>
    IsNotNull,

    /// <summary>Starts with.</summary>
    StartsWith,

    /// <summary>Ends with.</summary>
    EndsWith,

    /// <summary>Contains.</summary>
    Contains,

    /// <summary>Does not contain.</summary>
    DoesNotContain,

    /// <summary>Matches any item in a collection.</summary>
    In,
}
