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
        /// For relational databases, attempts to apply pending migrations first;
        /// falls back to EnsureCreated if no migrations are configured.
        /// For non-relational providers (e.g. InMemory), uses EnsureCreated.
        /// </summary>
        public DatabaseFacade EnsureDatabaseCreated()
        {
            ArgumentNullException.ThrowIfNull(databaseFacade);
            if (databaseFacade.IsRelational())
            {
                try
                {
                    if (databaseFacade.GetPendingMigrations().Any())
                        databaseFacade.Migrate();
                    else
                        _ = databaseFacade.EnsureCreated();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        "Failed to apply migrations or ensure the database is created.",
                        ex
                    );
                }
            }
            else
            {
                _ = databaseFacade.EnsureCreated();
            }

            return databaseFacade;
        }
    }
}
