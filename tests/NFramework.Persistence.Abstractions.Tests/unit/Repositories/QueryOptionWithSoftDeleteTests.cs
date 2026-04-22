using NFramework.Persistence.Abstractions.Repositories;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Unit.Repositories;

public class QueryOptionWithSoftDeleteTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Used as a generic type parameter in tests"
    )]
    private sealed class TestEntity
    {
        public Guid Id { get; set; }
    }

    [Fact]
    public void DefaultValues_ShouldIncludeDeletedFalse()
    {
        var options = new QueryOptionWithSoftDelete<TestEntity>();
        options.IncludeDeleted.ShouldBeFalse();
        options.Predicate.ShouldBeNull();
    }

    [Fact]
    public void CanSetIncludeDeleted()
    {
        var options = new QueryOptionWithSoftDelete<TestEntity> { IncludeDeleted = true };
        options.IncludeDeleted.ShouldBeTrue();
    }
}
