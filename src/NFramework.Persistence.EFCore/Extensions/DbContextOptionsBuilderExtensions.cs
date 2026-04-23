using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Interceptors;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Extension methods for configuring EF Core DbContext options.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    private static SoftDeletionInterceptor SoftDeleteInterceptorInstance { get; } = new();
    private static AuditableInterceptor AuditableInterceptorInstance { get; } = new();
    private static AuditLoggerInterceptor AuditLoggerInterceptorInstance { get; } = new();

    extension(DbContextOptionsBuilder builder)
    {
        /// <summary>
        /// Adds the NFramework soft delete interceptor to the DbContext options.
        /// </summary>
        public DbContextOptionsBuilder AddSoftDeleteInterceptor()
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddInterceptors(SoftDeleteInterceptorInstance);
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
        public DbContextOptionsBuilder<TContext> AddSoftDeleteInterceptor()
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddInterceptors(SoftDeleteInterceptorInstance);
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
