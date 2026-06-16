using NFramework.Persistence.Abstractions.Repositories;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Repositories;

public class QueryOptionTests
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
    public void QueryOption_DefaultValues_ShouldBeNulls()
    {
        var options = new QueryOption<TestEntity>();
        options.Predicate.ShouldBeNull();
        options.OrderBy.ShouldBeNull();
    }

    [Fact]
    public void QueryOption_ShouldDefaultToDefaultTracking()
    {
        var options = new QueryOption<TestEntity>();
        options.Tracking.ShouldBe(QueryTrackingMode.Default);
    }

    [Fact]
    public void QueryOption_ShouldDefaultToDefaultSplitting()
    {
        var options = new QueryOption<TestEntity>();
        options.Splitting.ShouldBe(QuerySplittingMode.Default);
    }

    [Fact]
    public void QueryOption_ShouldImplementIQuerySplitting()
    {
        var options = new QueryOption<TestEntity>(Splitting: QuerySplittingMode.SplitQuery);
        (options is IQuerySplitting).ShouldBeTrue();
        options.Splitting.ShouldBe(QuerySplittingMode.SplitQuery);
    }
}
