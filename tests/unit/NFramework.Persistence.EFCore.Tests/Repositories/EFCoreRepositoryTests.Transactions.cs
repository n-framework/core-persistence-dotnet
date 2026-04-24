using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

public class TransactionTests
{
    // Note: EF Core InMemory database does NOT support transactions.
    // We must use a SQLite in-memory database to test real transaction behavior.

    [Fact]
    public async Task CommitTransactionAsync_ShouldPersistChanges()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;
            using var context = new TestDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repo = new TestProductRepository(context);

            await repo.BeginTransactionAsync();

            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "TxProduct", Price = 1.0m });
            await repo.SaveChangesAsync();

            await repo.CommitTransactionAsync();

            int count = await repo.CountAsync();
            count.ShouldBe(1);
        }
        finally
        {
            await connection.CloseAsync();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task RollbackTransactionAsync_ShouldDiscardChanges()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;
            using var context = new TestDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repo = new TestProductRepository(context);

            await repo.BeginTransactionAsync();

            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "TxProduct", Price = 1.0m });
            await repo.SaveChangesAsync();

            await repo.RollbackTransactionAsync();

            int count = await repo.CountAsync();
            count.ShouldBe(0);
        }
        finally
        {
            await connection.CloseAsync();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task RollbackTransactionAsync_WithoutBegin_ShouldThrow()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;
            using var context = new TestDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repo = new TestProductRepository(context);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () => await repo.RollbackTransactionAsync());
        }
        finally
        {
            await connection.CloseAsync();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task CommitTransactionAsync_WithoutBegin_ShouldThrow()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;
            using var context = new TestDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repo = new TestProductRepository(context);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () => await repo.CommitTransactionAsync());
        }
        finally
        {
            await connection.CloseAsync();
            connection.Dispose();
        }
    }
}
