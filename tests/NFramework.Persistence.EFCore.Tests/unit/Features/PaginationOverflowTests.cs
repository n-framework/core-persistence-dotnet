using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Tests.Unit.Helpers;
using NFramework.Persistence.EFCore.Tests.Unit.Repositories;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Features;

public class PaginationOverflowTests
{
    [Fact]
    public async Task GetListAsync_WithOverflowingPaging_ShouldThrowArgumentOutOfRangeException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        // Intentionally create a Paging object that will cause (long)Index * Size to exceed int.MaxValue
        var paging = new Paging(int.MaxValue, 10);
        var options = new PageableQueryOption<TestProduct> { Page = paging };

        var ex = await Should.ThrowAsync<ArgumentOutOfRangeException>(async () => await repo.GetListAsync(options));

        ex.ParamName.ShouldBe("paging");
        ex.Message.ShouldContain("overflow of the maximum skipped items");
    }

    [Fact]
    public async Task GetListByDynamicAsync_WithOverflowingPaging_ShouldThrowArgumentOutOfRangeException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        var paging = new Paging(int.MaxValue / 2, 5); // Exceeds int.MaxValue
        var options = new PageableDynamicQueryOption { Page = paging };

        var ex = await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            await repo.GetListByDynamicAsync(options)
        );

        ex.ParamName.ShouldBe("paging");
    }
}
