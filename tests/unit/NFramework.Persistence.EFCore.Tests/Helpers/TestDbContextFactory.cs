using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Interceptors;

namespace NFramework.Persistence.EFCore.Tests.Helpers;

/// <summary>
/// Factory for creating TestDbContext instances for testing.
/// Provides both In-Memory and SQLite-backed contexts.
/// </summary>
internal static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new In-Memory TestDbContext with a unique database name.
    /// Ideal for fast unit tests that don't require relational features.
    /// </summary>
    public static TestDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new SoftDeletionInterceptor(), new AuditableInterceptor())
            .Options;

        var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a new SQLite-backed TestDbContext using an in-memory database.
    /// Ideal for tests that require relational behavior or RowVersion concurrency.
    /// </summary>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Connection is disposed via context life-cycle event."
    )]
    public static TestDbContext CreateSqlite()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new SoftDeletionInterceptor(), new AuditableInterceptor())
            .Options;

        var context = new TestDbContext(options);
        try
        {
            context.Database.EnsureCreated();

            // Ensure the connection is closed when the context is disposed
            context.Database.GetDbConnection().Disposed += (s, e) => connection.Dispose();

            return context;
        }
        catch
        {
            context.Dispose();
            connection.Dispose();
            throw;
        }
    }
}
