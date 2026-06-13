using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using UnionRailway;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

public class TransactionTests
{
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

            (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "TxProduct", Price = 1.0m })).Unwrap();
            (await repo.SaveChangesAsync()).Unwrap();

            await repo.CommitTransactionAsync();

            int count = (await repo.CountAsync()).Unwrap();
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

            (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "TxProduct", Price = 1.0m })).Unwrap();
            (await repo.SaveChangesAsync()).Unwrap();

            await repo.RollbackTransactionAsync();

            int count = (await repo.CountAsync()).Unwrap();
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

            await Should.ThrowAsync<InvalidOperationException>(async () => await repo.CommitTransactionAsync());
        }
        finally
        {
            await connection.CloseAsync();
            connection.Dispose();
        }
    }
}
