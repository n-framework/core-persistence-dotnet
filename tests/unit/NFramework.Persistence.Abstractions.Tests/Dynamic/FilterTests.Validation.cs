using NFramework.Persistence.Abstractions.Dynamic;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Dynamic;

public partial class FilterTests
{
    [Fact]
    public void Filter_EmptyField_ShouldThrow()
    {
        var filter = new Filter();
        Should.Throw<ArgumentException>(() => filter.Field = "").Message.ShouldContain("Field name cannot be empty");
    }

    [Fact]
    public void Filter_LogicWithoutFilters_ShouldThrow()
    {
        var filter = new Filter
        {
            Field = "Name",
            Operator = FilterOperator.Equal,
            Logic = FilterLogic.And,
        };
        filter.Validate().Any().ShouldBeTrue();
    }
}
