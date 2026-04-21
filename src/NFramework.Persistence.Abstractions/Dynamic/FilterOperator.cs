namespace NFramework.Persistence.Abstractions.Dynamic;

/// <summary>
/// Specifies the comparison operator for a dynamic query filter.
/// </summary>
public enum FilterOperator
{
    Eq,
    Neq,
    Lt,
    Lte,
    Gt,
    Gte,
    IsNull,
    IsNotNull,
    StartsWith,
    EndsWith,
    Contains,
    DoesNotContain,
    In,
}
