using NFramework.Persistence.Abstractions.Pagination;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Pagination;

public class PagingTests
{
    [Fact]
    public void Paging_DefaultValues()
    {
        var paging = Paging.Default;
        paging.Index.ShouldBe(0u);
        paging.Size.ShouldBe(10u);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(5, 50)]
    [InlineData(100, 100)] // Boundary check for MaxSize (100 is default max)
    public void Paging_ShouldInitializeWithCorrectValues(uint index, uint size)
    {
        var paging = new Paging(index, size);
        paging.Index.ShouldBe(index);
        paging.Size.ShouldBe(size);
    }

    [Fact]
    public void Paging_InitBypass_ShouldThrow()
    {
        Action act = () => _ = new Paging { Index = 0, Size = 0 };
        act.ShouldThrow<ArgumentException>().Message.ShouldContain("greater than 0");
    }
}
