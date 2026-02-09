using System;

namespace TicketManagement.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when optimistic locking detects a concurrency conflict
/// </summary>
public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message)
        : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ConcurrencyException(
        string entityName,
        object entityId,
        byte[]? expectedVersion = null,
        byte[]? actualVersion = null)
        : base(BuildMessage(entityName, entityId))
    {
        EntityName = entityName;
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public string? EntityName { get; }
    public object? EntityId { get; }
    public byte[]? ExpectedVersion { get; }
    public byte[]? ActualVersion { get; }

    private static string BuildMessage(string entityName, object entityId)
    {
        return $"{entityName} with ID '{entityId}' was modified by another user. Please refresh and try again.";
    }
}
