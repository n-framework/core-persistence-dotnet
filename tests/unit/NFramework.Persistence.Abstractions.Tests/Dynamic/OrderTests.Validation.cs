using NFramework.Persistence.Abstractions.Dynamic;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Dynamic;

public partial class OrderTests
{
    [Fact]
    public void Order_EmptyField_ShouldThrow()
    {
        var order = new Order();
        Should.Throw<ArgumentException>(() => order.Field = " ").Message.ShouldContain("Field name cannot be empty");
    }
}
