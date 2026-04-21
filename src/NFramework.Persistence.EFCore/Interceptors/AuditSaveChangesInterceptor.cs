using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using NFramework.Persistence.Abstractions.Entities;

namespace NFramework.Persistence.EFCore.Interceptors;

/// <summary>
/// EF Core interceptor that automatically manages audit timestamps,
/// soft-delete state, and cascade soft-delete via navigation traversal
/// before changes are saved to the database.
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
            return;

        DateTime now = DateTime.UtcNow;

        foreach (EntityEntry entry in context.ChangeTracker.Entries().ToList())
        {
            HandleAuditTimestamps(entry, now);

            if (entry.State == EntityState.Deleted && IsSoftDeletableEntry(entry))
                CascadeSoftDelete(context, entry, now);
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
            SetPropertyValue(entry, nameof(AuditableEntity<>.UpdatedAt), now);
    }

    private static void CascadeSoftDelete(DbContext context, EntityEntry entry, DateTime now)
    {
        MarkAsSoftDeleted(entry, now);
        CascadeToNavigations(context, entry, now);
    }

    private static void MarkAsSoftDeleted(EntityEntry entry, DateTime now)
    {
        if (IsAlreadySoftDeleted(entry))
        {
            entry.State = EntityState.Modified;
            return;
        }

        entry.State = EntityState.Modified;
        SetPropertyValue(entry, nameof(SoftDeletableEntity<>.IsDeleted), true);
        SetPropertyValue(entry, nameof(SoftDeletableEntity<>.DeletedAt), (DateTime?)now);
        SetPropertyValue(entry, nameof(AuditableEntity<>.UpdatedAt), now);
    }

    private static void CascadeToNavigations(DbContext context, EntityEntry entry, DateTime now)
    {
        IEnumerable<INavigation> navigations = entry
            .Metadata.GetNavigations()
            .Where(n =>
                !n.IsOnDependent
                && !n.TargetEntityType.IsOwned()
                && (
                    n.ForeignKey.DeleteBehavior == DeleteBehavior.Cascade
                    || n.ForeignKey.DeleteBehavior == DeleteBehavior.ClientCascade
                )
            );

        foreach (INavigationBase navigation in navigations)
        {
            if (navigation.PropertyInfo == null)
                continue;

            if (navigation.IsCollection)
                CascadeCollection(context, entry, navigation, now);
            else
                CascadeReference(context, entry, navigation, now);
        }
    }

    private static void CascadeCollection(
        DbContext context,
        EntityEntry entry,
        INavigationBase navigation,
        DateTime now
    )
    {
        CollectionEntry collectionEntry = entry.Collection(navigation.PropertyInfo!.Name);
        if (!collectionEntry.IsLoaded)
            collectionEntry.Load();

        if (collectionEntry.CurrentValue is not IEnumerable collection)
            return;

        foreach (object? item in collection)
        {
            if (item == null)
                continue;

            EntityEntry childEntry = context.Entry(item);
            if (IsSoftDeletableEntry(childEntry) && !IsAlreadySoftDeleted(childEntry))
                CascadeSoftDelete(context, childEntry, now);
        }
    }

    private static void CascadeReference(DbContext context, EntityEntry entry, INavigationBase navigation, DateTime now)
    {
        ReferenceEntry referenceEntry = entry.Reference(navigation.PropertyInfo!.Name);
        if (!referenceEntry.IsLoaded)
            referenceEntry.Load();

        if (referenceEntry.CurrentValue == null)
            return;

        EntityEntry childEntry = context.Entry(referenceEntry.CurrentValue);
        if (IsSoftDeletableEntry(childEntry) && !IsAlreadySoftDeleted(childEntry))
            CascadeSoftDelete(context, childEntry, now);
    }

    private static bool IsSoftDeletableEntry(EntityEntry entry)
    {
        return entry.Properties.Any(p => p.Metadata.Name == nameof(SoftDeletableEntity<>.IsDeleted));
    }

    private static bool IsAlreadySoftDeleted(EntityEntry entry)
    {
        return entry.Entity.GetType().GetProperty(nameof(SoftDeletableEntity<>.IsDeleted))?.GetValue(entry.Entity)
            is true;
    }

    private static void SetPropertyValue(EntityEntry entry, string propertyName, object? value)
    {
        PropertyEntry? prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);

        if (prop is { } p2)
            p2.CurrentValue = value;
    }
}
