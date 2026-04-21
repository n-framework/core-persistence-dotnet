using NFramework.Persistence.Abstractions.Pagination;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Pagination;

public class PaginateResultTests
{
    [Fact]
    public void PaginateResult_ShouldStoreItemsAndMeta()
    {
        var items = new List<string> { "a", "b" };
        var paging = new Paging(0, 10);
        var meta = new PagingMeta(paging, 2, 1);
        var list = new PaginatedList<string>(items, meta);

        list.Items.ShouldBeEquivalentTo(items);
        list.Meta.ShouldBe(meta);
    }
}
