namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Unit of Work interface for coordinating multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
