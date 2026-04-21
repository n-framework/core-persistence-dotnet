using NFramework.Persistence.Abstractions.Pagination;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Pagination;

public class PagingTests
{
    [Fact]
    public void Paging_DefaultValues()
    {
        var paging = new Paging(0, 10);
        paging.Index.ShouldBe(0u);
        paging.Size.ShouldBe(10u);
    }

    [Fact]
    public void Paging_ShouldStoreValues()
    {
        var paging = new Paging(5, 50);
        paging.Index.ShouldBe(5u);
        paging.Size.ShouldBe(50u);
    }
}
