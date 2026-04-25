using Microsoft.EntityFrameworkCore;

namespace NFramework.Persistence.EFCore.Repositories;

public abstract partial class EFCoreRepository<TEntity, TId, TContext>
{
    /// <inheritdoc />
    public IQueryable<TEntity> Query() => DbSet.AsNoTracking();
}
