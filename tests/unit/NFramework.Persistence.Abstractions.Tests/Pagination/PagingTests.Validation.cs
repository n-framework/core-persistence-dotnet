using NFramework.Persistence.Abstractions.Pagination;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Pagination;

public partial class PagingTests
{
    [Fact]
    public void Paging_SizeZero_ShouldThrow()
    {
        Should
            .Throw<ArgumentException>(() => new Paging(0, 0))
            .Message.ShouldContain("Page size must be greater than 0");
    }
}
