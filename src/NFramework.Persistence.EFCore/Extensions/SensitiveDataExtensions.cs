using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NFramework.Persistence.EFCore.Constants;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Domain object for storing sensitive data masking configuration in EF Core annotations.
/// </summary>
internal sealed record SensitiveDataConfiguration(char MaskChar, int KeepStartChars, int KeepEndChars);

/// <summary>
/// Extension methods for marking EF Core properties as sensitive data via annotations.
/// </summary>
public static class SensitiveDataExtensions
{
    extension<TProperty>(PropertyBuilder<TProperty> builder)
    {
        /// <summary>
        /// Marks this property as sensitive, causing its value to be masked in audit logs.
        /// </summary>
        public PropertyBuilder<TProperty> IsSensitiveData(
            char maskChar = '*',
            int keepStartChars = 0,
            int keepEndChars = 0
        )
        {
            return builder.HasAnnotation(
                AnnotationKeys.SensitiveData,
                new SensitiveDataConfiguration(maskChar, keepStartChars, keepEndChars)
            );
        }
    }
}
