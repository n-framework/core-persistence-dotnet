using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Repositories;

namespace NFramework.Persistence.EFCore.Extensions;

internal static class QuerySplittingExtensions
{
    extension<TEntity>(IQueryable<TEntity> query)
        where TEntity : class
    {
        public IQueryable<TEntity> ApplySplitting(IQuerySplitting? options) =>
            options is null
                ? query
                : options.Splitting switch
                {
                    QuerySplittingMode.SplitQuery => query.AsSplitQuery(),
                    QuerySplittingMode.SingleQuery => query.AsSingleQuery(),
                    QuerySplittingMode.Default => query,
                    _ => query,
                };
    }
}
