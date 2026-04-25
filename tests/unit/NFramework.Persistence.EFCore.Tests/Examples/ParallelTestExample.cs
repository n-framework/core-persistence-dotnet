using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Examples;

public class ParallelTestExample
{
    [Fact]
    public async Task Test_1_Should_Have_Isolated_Database()
    {
        using var context = TestDbContextFactory.CreateInMemory();
        context.Products.Add(new TestProduct(Guid.NewGuid()) { Name = "Product 1" });
        await context.SaveChangesAsync();

        context.Products.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Test_2_Should_Have_Isolated_Database()
    {
        using var context = TestDbContextFactory.CreateInMemory();
        context.Products.Add(new TestProduct(Guid.NewGuid()) { Name = "Product 2" });
        await context.SaveChangesAsync();

        context.Products.Count().ShouldBe(1);
    }
}
