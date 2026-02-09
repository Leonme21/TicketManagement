namespace TicketManagement.Application.Common.Exceptions;

/// <summary>
/// ✅ NEW: Exception for optimistic concurrency conflicts
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException()
        : base("The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.")
    {
    }

    public ConcurrencyException(string message)
        : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
