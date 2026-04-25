namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Defines tracking configuration for a query.
/// </summary>
public interface IQueryTracking
{
    /// <summary>
    /// Gets the tracking mode for the query.
    /// </summary>
    QueryTrackingMode Tracking { get; init; }
}
