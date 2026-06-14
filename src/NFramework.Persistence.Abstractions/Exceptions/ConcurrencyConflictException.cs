namespace NFramework.Persistence.Abstractions.Exceptions;

/// <summary>
/// Thrown when an optimistic concurrency conflict is detected during a save operation.
/// The entity was modified by another process between the time it was read and saved.
/// </summary>
/// <remarks>
/// Deprecated: Use <c>UnionError.Conflict</c> from UnionRailway instead.
/// Kept for backward compatibility with NFrameworkErrorMapping type-name detection.
/// </remarks>
#pragma warning disable S1133 // Kept for backward compatibility with NFrameworkErrorMapping type-name detection
[Obsolete("Use UnionError.Conflict from UnionRailway instead. This type will be removed in a future major version.")]
public sealed class ConcurrencyConflictException : Exception
#pragma warning restore S1133
{
    public string? EntityType { get; }
    public string? EntityId { get; }

#pragma warning disable CA1819 // Properties should not return arrays
    public byte[]? CurrentVersion { get; }
    public byte[]? OriginalVersion { get; }
#pragma warning restore CA1819 // Properties should not return arrays

    public ConcurrencyConflictException()
        : base("A concurrency conflict was detected. The entity was modified by another process.") { }

    public ConcurrencyConflictException(string message)
        : base(message) { }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException) { }

    public ConcurrencyConflictException(
        string message,
        string? entityType,
        string? entityId,
        byte[]? currentVersion,
        byte[]? originalVersion,
        Exception? innerException = null
    )
        : base(message, innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
        CurrentVersion = currentVersion;
        OriginalVersion = originalVersion;
    }
}
