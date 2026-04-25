using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using NFramework.Persistence.EFCore.Constants;
using NFramework.Persistence.EFCore.Extensions;

namespace NFramework.Persistence.EFCore.Interceptors;

/// <summary>
/// EF Core interceptor that logs detailed entity changes before they are saved to the database,
/// as well as any failures that occur during save.
/// Sensitive properties configured via EF Core metadata annotations are automatically masked.
/// </summary>
public sealed partial class AuditLoggerInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        LogChanges(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(eventData);

        LogChanges(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        LogFailure(eventData);

        base.SaveChangesFailed(eventData);
    }

    /// <inheritdoc />
    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(eventData);

        LogFailure(eventData);

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private static void LogChanges(DbContext? context)
    {
        if (context == null || !context.ChangeTracker.HasChanges())
            return;
        ILoggerFactory factory =
            context.GetService<ILoggerFactory>()
            ?? throw new InvalidOperationException(
                "Audit logging is enabled but ILoggerFactory is not configured in the DbContext."
            );

        ILogger logger = factory.CreateLogger<AuditLoggerInterceptor>();
        if (!logger.IsEnabled(LogLevel.Information))
            return;
        List<EntityEntry> entries =
        [
            .. context
                .ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted),
        ];

        if (entries.Count == 0)
            return;

        StringBuilder sb = new();
        _ = sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Saving {entries.Count} entity changes:");

        foreach (EntityEntry entry in entries)
        {
            _ = sb.AppendLine(
                System.Globalization.CultureInfo.InvariantCulture,
                $"  - {entry.Metadata.Name} [{entry.State}]"
            );

            if (entry.State == EntityState.Modified)
            {
                foreach (PropertyEntry property in entry.Properties)
                {
                    if (property.IsModified)
                    {
                        string originalDisplay = FormatPropertyValue(property, property.OriginalValue);
                        string currentDisplay = FormatPropertyValue(property, property.CurrentValue);
                        _ = sb.AppendLine(
                            System.Globalization.CultureInfo.InvariantCulture,
                            $"      {property.Metadata.Name}: '{originalDisplay}' -> '{currentDisplay}'"
                        );
                    }
                }
            }
            else if (entry.State == EntityState.Added)
            {
                foreach (PropertyEntry property in entry.Properties)
                {
                    string display = FormatPropertyValue(property, property.CurrentValue);
                    _ = sb.AppendLine(
                        System.Globalization.CultureInfo.InvariantCulture,
                        $"      {property.Metadata.Name}: '{display}'"
                    );
                }
            }
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
#pragma warning disable CA1873
            LogAuditInfo(logger, sb.ToString());
#pragma warning restore CA1873
        }
    }

    private static string FormatPropertyValue(PropertyEntry property, object? value)
    {
        if (value == null)
            return string.Empty;

        var annotation = property.Metadata.FindAnnotation(AnnotationKeys.SensitiveData);
        if (annotation?.Value is not SensitiveDataConfiguration config)
            return value.ToString() ?? string.Empty;

        string stringValue = value.ToString() ?? string.Empty;
        return MaskValue(stringValue, config);
    }

    private static string MaskValue(string value, SensitiveDataConfiguration config)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        int keepStart = config.KeepStartChars;
        int keepEnd = config.KeepEndChars;

        if (keepStart + keepEnd >= value.Length)
            return new string(config.MaskChar, value.Length);

        int maskLength = value.Length - keepStart - keepEnd;
        char[] result = value.ToCharArray();
        for (int i = keepStart; i < keepStart + maskLength; i++)
            result[i] = config.MaskChar;

        return new string(result);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{AuditLog}")]
    private static partial void LogAuditInfo(ILogger logger, string auditLog);

    private static void LogFailure(DbContextErrorEventData eventData)
    {
        if (eventData.Context == null)
            return;

        ILoggerFactory factory =
            eventData.Context.GetService<ILoggerFactory>()
            ?? throw new InvalidOperationException(
                "Audit logging is enabled but ILoggerFactory is not configured in the DbContext."
            );

        ILogger logger = factory.CreateLogger<AuditLoggerInterceptor>();
        LogSaveChangesFailed(logger, eventData.Context.ContextId.InstanceId, eventData.Exception);
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "SaveChanges failed for context {ContextId}")]
    private static partial void LogSaveChangesFailed(ILogger logger, Guid contextId, Exception exception);
}
