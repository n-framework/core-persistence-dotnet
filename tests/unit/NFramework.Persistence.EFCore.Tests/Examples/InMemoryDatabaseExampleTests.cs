using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Examples;

public class InMemoryDatabaseExampleTests
{
    [Fact]
    public async Task InMemory_Database_Should_Work_For_Fast_Tests()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemory();
        var product = new TestProduct { Name = "Fast Test Product" };

        // Act
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Assert
        var savedProduct = await context.Products.FindAsync(product.Id);
        savedProduct.ShouldNotBeNull();
        savedProduct.Name.ShouldBe("Fast Test Product");
    }
}
