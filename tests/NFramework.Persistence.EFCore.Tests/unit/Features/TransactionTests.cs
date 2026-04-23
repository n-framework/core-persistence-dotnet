using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Tests.Unit.Helpers;
using NFramework.Persistence.EFCore.Tests.Unit.Repositories;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Features;

public class TransactionTests
{
    // Note: EF Core InMemory database does NOT support transactions.
    // We must use a SQLite in-memory database to test real transaction behavior.

#pragma warning disable CA2000 // Dispose objects before losing scope
    private static TestDbContext CreateSqliteContext()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;
        var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    [Fact]
    public async Task CommitTransactionAsync_ShouldPersistChanges()
    {
        using var context = CreateSqliteContext();
        var repo = new TestProductRepository(context);

        await repo.BeginTransactionAsync();

        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "TxProduct",
                Price = 1.0m,
            }
        );
        await repo.SaveChangesAsync();

        await repo.CommitTransactionAsync();

        int count = await repo.CountAsync();
        count.ShouldBe(1);
    }

    [Fact]
    public async Task RollbackTransactionAsync_ShouldDiscardChanges()
    {
        using var context = CreateSqliteContext();
        var repo = new TestProductRepository(context);

        await repo.BeginTransactionAsync();

        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "TxProduct",
                Price = 1.0m,
            }
        );
        await repo.SaveChangesAsync();

        await repo.RollbackTransactionAsync();

        int count = await repo.CountAsync();
        count.ShouldBe(0);
    }

    [Fact]
    public async Task RollbackTransactionAsync_WithoutBegin_ShouldThrow()
    {
        using var context = CreateSqliteContext();
        var repo = new TestProductRepository(context);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await repo.RollbackTransactionAsync());
    }

    [Fact]
    public async Task CommitTransactionAsync_WithoutBegin_ShouldThrow()
    {
        using var context = CreateSqliteContext();
        var repo = new TestProductRepository(context);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await repo.CommitTransactionAsync());
    }
}
