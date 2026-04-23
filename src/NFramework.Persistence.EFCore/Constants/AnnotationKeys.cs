namespace NFramework.Persistence.EFCore.Constants;

/// <summary>
/// Well-known annotation keys used by NFramework EF Core interceptors.
/// </summary>
public static class AnnotationKeys
{
    /// <summary>
    /// Annotation key indicating a property contains sensitive data that should be masked in audit logs.
    /// The annotation value is a <see cref="NFramework.Persistence.EFCore.Extensions.SensitiveDataConfiguration"/>.
    /// </summary>
    public const string SensitiveData = "NFramework:SensitiveData";
}
