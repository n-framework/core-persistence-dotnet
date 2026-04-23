using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Repositories;

namespace NFramework.Persistence.EFCore.Extensions;

internal static class QueryTrackingExtensions
{
    extension<TEntity>(IQueryable<TEntity> query)
        where TEntity : class
    {
        public IQueryable<TEntity> ApplyTracking(IQueryTracking? options) =>
            options is null
                ? query
                : options.Tracking switch
                {
                    QueryTrackingMode.NoTracking => query.AsNoTracking(),
                    QueryTrackingMode.Tracking => query.AsTracking(),
                    QueryTrackingMode.NoTrackingWithIdentityResolution => query.AsNoTrackingWithIdentityResolution(),
                    QueryTrackingMode.Default => query,
                    _ => query,
                };
    }
}
