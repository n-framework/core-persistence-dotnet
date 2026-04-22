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

        DateTime now = DateTime.UtcNow;
        foreach (EntityEntry entry in GetEntriesToSoftDelete(eventData.Context, now))
            CascadeSoftDelete(eventData.Context!, entry, now);

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(eventData);

        DateTime now = DateTime.UtcNow;
        foreach (EntityEntry entry in GetEntriesToSoftDelete(eventData.Context, now))
            await CascadeSoftDeleteAsync(eventData.Context!, entry, now, cancellationToken).ConfigureAwait(false);

        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private static List<EntityEntry> GetEntriesToSoftDelete(DbContext? context, DateTime now)
    {
        if (context == null)
            return [];

        List<EntityEntry> softDeleteEntries = [];

        foreach (EntityEntry entry in context.ChangeTracker.Entries().ToList())
        {
            HandleAuditTimestamps(entry, now);

            if (entry.State == EntityState.Deleted && IsSoftDeletableEntry(entry))
                softDeleteEntries.Add(entry);
        }

        return softDeleteEntries;
    }

    private static void HandleAuditTimestamps(EntityEntry entry, DateTime now)
    {
        if (entry.Entity is IAuditableEntity auditable)
        {
            if (entry.State == EntityState.Added)
            {
                auditable.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                auditable.UpdatedAt = now;
            }
        }
    }

    private static void MarkAsSoftDeleted(EntityEntry entry, DateTime now)
    {
        if (IsAlreadySoftDeleted(entry))
        {
            entry.State = EntityState.Modified;
            return;
        }

        entry.State = EntityState.Modified;
        if (entry.Entity is ISoftDeletableEntity softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = now;
        }

        if (entry.Entity is IAuditableEntity auditable)
        {
            auditable.UpdatedAt = now;
        }
    }

    private static IEnumerable<INavigationBase> GetCascadeNavigations(EntityEntry entry)
    {
        IEnumerable<INavigationBase> directNavigations = entry
            .Metadata.GetNavigations()
            .Where(n =>
                !n.IsOnDependent
                && !n.TargetEntityType.IsOwned()
                && (
                    n.ForeignKey.DeleteBehavior == DeleteBehavior.Cascade
                    || n.ForeignKey.DeleteBehavior == DeleteBehavior.ClientCascade
                )
            );

        return directNavigations.Where(n => n.PropertyInfo != null);
    }

    private static IEnumerable<EntityEntry> GetValidChildren(DbContext context, object? currentValue)
    {
        if (currentValue is not IEnumerable collection)
            yield break;

        foreach (object? item in collection)
        {
            if (item == null)
                continue;

            EntityEntry childEntry = context.Entry(item);
            if (IsSoftDeletableEntry(childEntry) && !IsAlreadySoftDeleted(childEntry))
                yield return childEntry;
        }
    }

    private static EntityEntry? GetValidChild(DbContext context, object? currentValue)
    {
        if (currentValue == null)
            return null;

        EntityEntry childEntry = context.Entry(currentValue);
        return IsSoftDeletableEntry(childEntry) && !IsAlreadySoftDeleted(childEntry) ? childEntry : null;
    }

    private static void CascadeSoftDelete(DbContext context, EntityEntry entry, DateTime now)
    {
        MarkAsSoftDeleted(entry, now);

        foreach (INavigationBase navigation in GetCascadeNavigations(entry))
        {
            if (navigation.IsCollection)
            {
                CollectionEntry collectionEntry = entry.Collection(navigation.PropertyInfo!.Name);
                if (!collectionEntry.IsLoaded)
                {
                    try
                    {
                        collectionEntry.Load();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to load collection navigation '{navigation.PropertyInfo!.Name}' for entity '{entry.Metadata.Name}'.",
                            ex
                        );
                    }
                }

                foreach (EntityEntry childEntry in GetValidChildren(context, collectionEntry.CurrentValue))
                    CascadeSoftDelete(context, childEntry, now);
            }
            else
            {
                ReferenceEntry referenceEntry = entry.Reference(navigation.PropertyInfo!.Name);
                if (!referenceEntry.IsLoaded)
                {
                    try
                    {
                        referenceEntry.Load();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to load reference navigation '{navigation.PropertyInfo!.Name}' for entity '{entry.Metadata.Name}'.",
                            ex
                        );
                    }
                }

                if (GetValidChild(context, referenceEntry.CurrentValue) is { } childEntry)
                    CascadeSoftDelete(context, childEntry, now);
            }
        }
    }

    private static async Task CascadeSoftDeleteAsync(
        DbContext context,
        EntityEntry entry,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        MarkAsSoftDeleted(entry, now);

        foreach (INavigationBase navigation in GetCascadeNavigations(entry))
        {
            if (navigation.IsCollection)
            {
                CollectionEntry collectionEntry = entry.Collection(navigation.PropertyInfo!.Name);
                if (!collectionEntry.IsLoaded)
                {
                    try
                    {
                        await collectionEntry.LoadAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to load collection navigation '{navigation.PropertyInfo!.Name}' for entity '{entry.Metadata.Name}'.",
                            ex
                        );
                    }
                }

                foreach (EntityEntry childEntry in GetValidChildren(context, collectionEntry.CurrentValue))
                    await CascadeSoftDeleteAsync(context, childEntry, now, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                ReferenceEntry referenceEntry = entry.Reference(navigation.PropertyInfo!.Name);
                if (!referenceEntry.IsLoaded)
                {
                    try
                    {
                        await referenceEntry.LoadAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to load reference navigation '{navigation.PropertyInfo!.Name}' for entity '{entry.Metadata.Name}'.",
                            ex
                        );
                    }
                }

                if (GetValidChild(context, referenceEntry.CurrentValue) is { } childEntry)
                    await CascadeSoftDeleteAsync(context, childEntry, now, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static bool IsSoftDeletableEntry(EntityEntry entry)
    {
        return entry.Entity is ISoftDeletableEntity;
    }

    private static bool IsAlreadySoftDeleted(EntityEntry entry)
    {
        return entry.Entity is ISoftDeletableEntity softDeletable && softDeletable.IsDeleted;
    }
}
