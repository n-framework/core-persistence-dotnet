using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Extensions;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Extensions;

public class DatabaseFacadeExtensionsTests
{
    private sealed class RelationalTestDbContext(DbContextOptions<RelationalTestDbContext> options)
        : DbContext(options);

    [Fact]
    public void EnsureDatabaseCreated_Relational_ShouldNotThrow()
    {
        // Use using for the connection to fix CA2000
        using SqliteConnection connection = new("DataSource=:memory:");
        connection.Open();

        DbContextOptions<RelationalTestDbContext> options = new DbContextOptionsBuilder<RelationalTestDbContext>()
            .UseSqlite(connection)
            .Options;

        using RelationalTestDbContext context = new(options);

        // Act & Assert (Should not throw and should create db)
        _ = Should.NotThrow(() => context.Database.EnsureDatabaseCreated());
    }

    private sealed class InMemoryTestDbContext(DbContextOptions<InMemoryTestDbContext> options) : DbContext(options) { }

    [Fact]
    public void EnsureDatabaseCreated_InMemory_ShouldNotThrow()
    {
        DbContextOptions<InMemoryTestDbContext> options = new DbContextOptionsBuilder<InMemoryTestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using InMemoryTestDbContext context = new(options);

        // Act & Assert
        _ = Should.NotThrow(() => context.Database.EnsureDatabaseCreated());
    }
}
