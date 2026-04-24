using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NFramework.Persistence.EFCore.Repositories;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

public class BulkOperationErrorTests
{
    private sealed class BulkTestRepository(TestDbContext context)
        : EFCoreRepository<TestProduct, Guid, TestDbContext>(context);

    [Fact]
    public async Task BulkAddAsync_WithNullItems_ShouldLogWarningAndSkip()
    {
        // Arrange
        using var context = TestDbContext.Create();
        var repo = new BulkTestRepository(context);

        // Act & Assert
        await repo.BulkAddAsync(new TestProduct[] { null! }).ShouldNotThrowAsync();
    }

    [Fact]
    public async Task BulkUpdateAsync_WithNullItems_ShouldLogWarningAndSkip()
    {
        // Arrange
        using var context = TestDbContext.Create();
        var repo = new BulkTestRepository(context);

        // Act & Assert
        await repo.BulkUpdateAsync(new TestProduct[] { null! }).ShouldNotThrowAsync();
    }

    [Fact]
    public async Task BulkDeleteAsync_WithNullItems_ShouldLogWarningAndSkip()
    {
        // Arrange
        using var context = TestDbContext.Create();
        var repo = new BulkTestRepository(context);

        // Act & Assert
        await repo.BulkDeleteAsync(new TestProduct[] { null! }).ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Transaction_WhenCommitFails_ShouldThrowOriginalException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var mockContext = new Mock<TestDbContext>(options);
        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);

        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

        // IDbContextTransaction needs a mock too
        var mockTransaction = new Mock<IDbContextTransaction>();

        mockDatabase.Setup(d => d.CurrentTransaction).Returns(mockTransaction.Object);
        mockDatabase
            .Setup(d => d.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database failure"));

        var repo = new BulkTestRepository(mockContext.Object);

        // Act & Assert
        var ex = await Should.ThrowAsync<Exception>(async () => await repo.CommitTransactionAsync());
        ex.Message.ShouldContain("Database failure");
    }
}
