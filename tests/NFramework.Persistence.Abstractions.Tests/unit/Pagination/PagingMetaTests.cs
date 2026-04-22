using NFramework.Persistence.Abstractions.Pagination;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Unit.Pagination;

public class PagingMetaTests
{
    [Fact]
    public void PagingMeta_ShouldCalculateNavigationProperties()
    {
        var paging = new Paging(1, 10);
        var meta = new PagingMeta(paging, 25, 3);

        meta.HasPrevious.ShouldBeTrue();
        meta.HasNext.ShouldBeTrue();
        meta.TotalCount.ShouldBe(25);
        meta.TotalPages.ShouldBe(3);
    }

    [Fact]
    public void PagingMeta_FirstPage_ShouldNotHavePrevious()
    {
        var paging = new Paging(0, 10);
        var meta = new PagingMeta(paging, 25, 3);

        meta.HasPrevious.ShouldBeFalse();
        meta.HasNext.ShouldBeTrue();
    }

    [Fact]
    public void PagingMeta_LastPage_ShouldNotHaveNext()
    {
        var paging = new Paging(2, 10);
        var meta = new PagingMeta(paging, 25, 3);

        meta.HasPrevious.ShouldBeTrue();
        meta.HasNext.ShouldBeFalse();
    }
}
