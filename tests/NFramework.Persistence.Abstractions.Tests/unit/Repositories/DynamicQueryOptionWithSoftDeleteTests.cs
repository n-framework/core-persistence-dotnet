using NFramework.Persistence.Abstractions.Repositories;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Unit.Repositories;

public class DynamicQueryOptionWithSoftDeleteTests
{
    [Fact]
    public void DefaultValues_ShouldIncludeDeletedFalse()
    {
        var options = new DynamicQueryOptionWithSoftDelete();
        options.IncludeDeleted.ShouldBeFalse();
        options.Filters.ShouldBeNull();
        options.Orders.ShouldBeNull();
    }

    [Fact]
    public void CanSetIncludeDeleted()
    {
        var options = new DynamicQueryOptionWithSoftDelete { IncludeDeleted = true };
        options.IncludeDeleted.ShouldBeTrue();
    }
}
