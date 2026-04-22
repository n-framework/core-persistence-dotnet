namespace NFramework.Persistence.Abstractions.Exceptions;

/// <summary>
/// Thrown when an optimistic concurrency conflict is detected during a save operation.
/// The entity was modified by another process between the time it was read and saved.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("A concurrency conflict was detected. The entity was modified by another process.") { }

    public ConcurrencyConflictException(string message)
        : base(message) { }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
