using NFramework.Persistence.Abstractions.Dynamic;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Dynamic;

public class OrderTests
{
    [Fact]
    public void Order_DefaultValues()
    {
        var order = new Order();
        order.Field.ShouldBeEmpty();
        order.Direction.ShouldBe(OrderDirection.Asc);
    }

    [Fact]
    public void Order_CanSetProperties()
    {
        var order = new Order { Field = "Name", Direction = OrderDirection.Desc };
        order.Field.ShouldBe("Name");
        order.Direction.ShouldBe(OrderDirection.Desc);
    }
}
