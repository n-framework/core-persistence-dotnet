using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Interceptors;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Extension methods for configuring EF Core DbContext options.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    private static AuditableInterceptor AuditableInterceptorInstance { get; } = new();
    private static AuditLoggerInterceptor AuditLoggerInterceptorInstance { get; } = new();

    extension(DbContextOptionsBuilder builder)
    {
        /// <summary>
        /// Adds the NFramework soft delete interceptor to the DbContext options.
        /// </summary>
        /// <param name="maxCascadeDepth">The maximum depth allowed for cascade soft-delete traversal. Defaults to 50.</param>
        public DbContextOptionsBuilder AddSoftDeleteInterceptor(int? maxCascadeDepth = 50)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddInterceptors(new SoftDeletionInterceptor { MaxCascadeDepth = maxCascadeDepth });
        }

        /// <summary>
        /// Adds the NFramework auditable interceptor to the DbContext options.
        /// </summary>
        public DbContextOptionsBuilder AddAuditableInterceptor()
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddInterceptors(AuditableInterceptorInstance);
        }

        /// <summary>
        /// Adds the NFramework audit logger interceptor to the DbContext options.
        /// </summary>
        public DbContextOptionsBuilder AddAuditLoggerInterceptor()
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddInterceptors(AuditLoggerInterceptorInstance);
        }
    }

    extension<TContext>(DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext
    {
        /// <summary>
        /// Adds the NFramework soft delete interceptor to the DbContext options.
        /// </summary>
        /// <param name="maxCascadeDepth">The maximum depth allowed for cascade soft-delete traversal. Defaults to 50.</param>
        public DbContextOptionsBuilder<TContext> AddSoftDeleteInterceptor(int? maxCascadeDepth = 50)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddInterceptors(new SoftDeletionInterceptor { MaxCascadeDepth = maxCascadeDepth });
        }

        /// <summary>
        /// Adds the NFramework auditable interceptor to the DbContext options.
        /// </summary>
        public DbContextOptionsBuilder<TContext> AddAuditableInterceptor()
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddInterceptors(AuditableInterceptorInstance);
        }

        /// <summary>
        /// Adds the NFramework audit logger interceptor to the DbContext options.
        /// </summary>
        public DbContextOptionsBuilder<TContext> AddAuditLoggerInterceptor()
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddInterceptors(AuditLoggerInterceptorInstance);
        }
    }
}
