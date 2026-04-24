using NFramework.Persistence.Abstractions.Repositories;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Repositories;

public class PageableQueryOptionTests
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
    public void DefaultValues_ShouldIncludeDefaultPaging()
    {
        var options = new PageableQueryOption<TestEntity>();
        options.Predicate.ShouldBeNull();
        options.Page.Index.ShouldBe(0u);
        options.Page.Size.ShouldBe(10u);
    }
}
