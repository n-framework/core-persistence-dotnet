using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Repositories;

public class PageableDynamicQueryOptionTests
{
    [Fact]
    public void DefaultValues_ShouldIncludeDefaultPaging()
    {
        var query = new PageableDynamicQueryOption();
        query.Filters.ShouldBeNull();
        query.Orders.ShouldBeNull();
        query.Page.Index.ShouldBe(0u);
        query.Page.Size.ShouldBe(10u);
    }

    [Fact]
    public void PageableDynamicQueryOption_WithInitSyntax_ShouldWork()
    {
        var query = new PageableDynamicQueryOption
        {
            Filters =
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.Equal,
                    Value = "test",
                },
            ],
            Orders = [new Order { Field = "CreatedAt", Direction = OrderDirection.Desc }],
            Page = new Paging(2, 25),
        };

        query.Filters.Count.ShouldBe(1);
        query.Page.Index.ShouldBe(2u);
        query.Page.Size.ShouldBe(25u);
    }
}
