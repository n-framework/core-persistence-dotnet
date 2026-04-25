namespace NFramework.Persistence.Abstractions.Repositories;

/// <summary>
/// Specifies how results of a query are tracked by the persistence context.
/// </summary>
public enum QueryTrackingMode
{
    /// <summary>
    /// Uses the default tracking behavior of the underlying persistence provider.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Entities are not tracked by the context. Best for performance in read-only scenarios.
    /// </summary>
    NoTracking = 1,

    /// <summary>
    /// Entities are tracked by the context. Changes to entities will be detected and can be saved.
    /// </summary>
    Tracking = 2,

    /// <summary>
    /// Entities are not tracked, but identity resolution is performed to ensure consistent object graphs.
    /// </summary>
    NoTrackingWithIdentityResolution = 3,
}
