using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NFramework.Persistence.Abstractions.Entities;

namespace NFramework.Persistence.EFCore.Interceptors;

/// <summary>
/// EF Core interceptor that automatically manages audit timestamps
/// (CreatedAt and UpdatedAt) before changes are saved to the database.
/// </summary>
public sealed class AuditableInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        UpdateTimestamps(eventData.Context);

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

        UpdateTimestamps(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateTimestamps(DbContext? context)
    {
        if (context == null || !context.ChangeTracker.HasChanges())
            return;

        DateTime now = DateTime.UtcNow;

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditableEntity auditable)
            {
                if (entry.State == EntityState.Added)
                    auditable.CreatedAt = now;
                else if (entry.State == EntityState.Modified)
                    auditable.UpdatedAt = now;
            }
        }
    }
}
