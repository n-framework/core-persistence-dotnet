using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Extensions;

public class NullHandlingTests
{
    [Fact]
    public async Task AddAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        await Should.ThrowAsync<ArgumentNullException>(async () => await repo.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        await Should.ThrowAsync<ArgumentNullException>(async () => await repo.UpdateAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        await Should.ThrowAsync<ArgumentNullException>(async () => await repo.DeleteAsync(null!));
    }

    [Fact]
    public async Task UpsertAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        await Should.ThrowAsync<ArgumentNullException>(async () => await repo.UpsertAsync(null!));
    }

    [Fact]
    public async Task BulkAddAsync_WithNullCollection_ShouldThrowArgumentNullException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        await Should.ThrowAsync<ArgumentNullException>(async () => await repo.BulkAddAsync(null!));
    }
}
