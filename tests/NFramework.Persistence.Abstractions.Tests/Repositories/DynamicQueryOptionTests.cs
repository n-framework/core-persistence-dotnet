using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.Abstractions.Repositories;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Repositories;

public class DynamicQueryOptionTests
{
    [Fact]
    public void DefaultValues_ShouldBeEmpty()
    {
        var query = new DynamicQueryOption();
        query.Filters.ShouldBeNull();
        query.Orders.ShouldBeNull();
    }

    [Fact]
    public void DynamicQueryOption_WithInitSyntax_ShouldWork()
    {
        var query = new DynamicQueryOption
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
        };

        query.Filters.Count.ShouldBe(1);
        query.Orders.Count.ShouldBe(1);
    }
}
