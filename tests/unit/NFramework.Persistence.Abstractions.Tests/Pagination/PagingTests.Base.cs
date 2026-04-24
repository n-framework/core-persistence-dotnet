using NFramework.Persistence.Abstractions.Pagination;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Pagination;

public partial class PagingTests
{
    [Fact]
    public void Paging_DefaultValues()
    {
        var paging = Paging.Default;
        paging.Index.ShouldBe(0);
        paging.Size.ShouldBe(10);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(5, 50)]
    [InlineData(100, 100)] // Boundary check for MaxSize (100 is default max)
    public void Paging_ShouldInitializeWithCorrectValues(int index, int size)
    {
        var paging = new Paging(index, size);
        paging.Index.ShouldBe(index);
        paging.Size.ShouldBe(size);
    }

    [Fact]
    public void Paging_InitBypass_ShouldThrow()
    {
        Action act = () => _ = new Paging { Index = 0, Size = 0 };
        act.ShouldThrow<ArgumentOutOfRangeException>().Message.ShouldContain("greater than 0");
    }

    [Fact]
    public void Paging_NegativeIndex_ShouldThrow()
    {
        Action act = () => _ = new Paging(-1, 10);
        act.ShouldThrow<ArgumentOutOfRangeException>().Message.ShouldContain("greater than or equal to 0");
    }
}
