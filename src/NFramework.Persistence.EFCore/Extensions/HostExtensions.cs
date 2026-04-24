using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IHost"/> to handle persistence-related startup tasks.
/// </summary>
public static partial class HostExtensions
{
    extension(IHost host)
    {
        /// <summary>
        /// Automatically applies any pending migrations for the specified <see cref="DbContext"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of DbContext to migrate.</typeparam>
        /// <returns>The host for chaining.</returns>
        [RequiresDynamicCode(
            "Automatic migrations at startup require building the design-time model, which is not supported with Native AOT. Consider using Migration Bundles."
        )]
        [RequiresUnreferencedCode(
            "Automatic migrations at startup require building the design-time model, which is not supported with Native AOT. Consider using Migration Bundles."
        )]
        public async Task<IHost> ApplyMigrationsAsync<
            [DynamicallyAccessedMembers(
                DynamicallyAccessedMemberTypes.PublicConstructors
                    | DynamicallyAccessedMemberTypes.NonPublicConstructors
                    | DynamicallyAccessedMemberTypes.PublicFields
                    | DynamicallyAccessedMemberTypes.NonPublicFields
                    | DynamicallyAccessedMemberTypes.PublicProperties
                    | DynamicallyAccessedMemberTypes.NonPublicProperties
                    | DynamicallyAccessedMemberTypes.Interfaces
            )]
                TContext
        >()
            where TContext : DbContext
        {
            using IServiceScope scope = host.Services.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();

            using TContext context = services.GetRequiredService<TContext>();
            IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

            await strategy
                .ExecuteAsync(async () =>
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        Log.PreparingDatabase(logger, typeof(TContext).Name);

                    await context.Database.MigrateDatabaseAsync().ConfigureAwait(false);

                    if (logger.IsEnabled(LogLevel.Information))
                        Log.DatabasePreparationCompleted(logger, typeof(TContext).Name);
                })
                .ConfigureAwait(false);

            return host;
        }
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Preparing database for {ContextName}...")]
        public static partial void PreparingDatabase(ILogger logger, string contextName);

        [LoggerMessage(2, LogLevel.Information, "Database preparation completed for {ContextName}.")]
        public static partial void DatabasePreparationCompleted(ILogger logger, string contextName);
    }
}
