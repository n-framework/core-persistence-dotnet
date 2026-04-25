using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Provides extension methods for EF Core's <see cref="DatabaseFacade"/>.
/// </summary>
public static class DatabaseFacadeExtensions
{
    extension(DatabaseFacade databaseFacade)
    {
        /// <summary>
        /// Ensures the database is created properly based on the provider type.
        /// </summary>
        [RequiresDynamicCode("Database creation and migrations are not supported with Native AOT.")]
        [RequiresUnreferencedCode("Database creation and migrations are not supported with Native AOT.")]
        public async Task MigrateDatabaseAsync()
        {
            if (databaseFacade.IsRelational() && databaseFacade.GetMigrations().Any())
                await databaseFacade.MigrateAsync().ConfigureAwait(false);
            else
                _ = await databaseFacade.EnsureCreatedAsync().ConfigureAwait(false);
        }
    }
}
