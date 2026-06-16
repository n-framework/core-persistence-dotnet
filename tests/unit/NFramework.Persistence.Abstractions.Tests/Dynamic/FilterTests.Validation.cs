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

    [Fact]
    public void Filter_NestingExceedsMaxDepth_ShouldFail()
    {
        Filter root = CreateNestedFilter(Filter.MaxDepth + 1);

        var errors = root.Validate().ToList();

        errors.ShouldNotBeEmpty();
        errors.ShouldContain(err => err.Contains("nesting depth exceeds"));
    }

    [Fact]
    public void Filter_NestingAtMaxDepth_ShouldSucceed()
    {
        Filter root = CreateNestedFilter(Filter.MaxDepth);

        var errors = root.Validate().ToList();

        errors.ShouldBeEmpty();
    }

    private static Filter CreateNestedFilter(int depth)
    {
        Filter leaf = new()
        {
            Field = "Name",
            Operator = FilterOperator.Equal,
            Value = "test",
        };

        Filter current = leaf;
        for (int i = 0; i < depth; i++)
        {
            current = new Filter(FilterLogic.And, [current]);
        }

        return current;
    }
}
