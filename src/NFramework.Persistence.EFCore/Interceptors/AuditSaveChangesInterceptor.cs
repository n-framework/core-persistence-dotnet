using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NFramework.Persistence.Abstractions.Entities;

namespace NFramework.Persistence.EFCore.Interceptors;

/// <summary>
/// EF Core interceptor that automatically manages audit timestamps
/// and soft-delete state before changes are saved to the database.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        ApplyAuditRules(eventData.Context);
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
        ApplyAuditRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyAuditRules(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;

        foreach (EntityEntry entry in context.ChangeTracker.Entries().ToList())
        {
            HandleAuditTimestamps(entry, now);
            ConvertDeleteToSoftDelete(entry, now);
        }
    }

    private static void HandleAuditTimestamps(EntityEntry entry, DateTime now)
    {
        if (entry.State == EntityState.Added)
        {
            SetPropertyValue(entry, nameof(AuditableEntity<>.CreatedAt), now);
            SetPropertyValue(entry, nameof(AuditableEntity<>.UpdatedAt), now);
        }
        else if (entry.State == EntityState.Modified)
        {
            SetPropertyValue(entry, nameof(AuditableEntity<>.UpdatedAt), now);
        }
    }

    private static void ConvertDeleteToSoftDelete(EntityEntry entry, DateTime now)
    {
        if (entry.State != EntityState.Deleted)
        {
            return;
        }

        PropertyEntry? isDeletedProp = entry.Properties.FirstOrDefault(p =>
            p.Metadata.Name == nameof(SoftDeletableEntity<>.IsDeleted)
        );

        if (isDeletedProp == null)
        {
            return;
        }

        entry.State = EntityState.Modified;
        isDeletedProp.CurrentValue = true;

        SetPropertyValue(entry, nameof(SoftDeletableEntity<>.DeletedAt), (DateTime?)now);
    }

    private static void SetPropertyValue(EntityEntry entry, string propertyName, object? value)
    {
        PropertyEntry? prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);

        if (prop is { } p2)
        {
            p2.CurrentValue = value;
        }
    }
}
