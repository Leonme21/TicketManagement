using System.Collections.Generic;
using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.ValueObjects;

public sealed class TicketDescription
{
    public string Value { get; }

    private TicketDescription(string value)
    {
        Value = value;
    }

    public static Result<TicketDescription> Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result<TicketDescription>.Invalid("Ticket description cannot be empty");

        description = description.Trim();

        if (description.Length > 5000)
            return Result<TicketDescription>.Invalid("Ticket description must not exceed 5000 characters");

        return Result<TicketDescription>.Success(new TicketDescription(description));
    }

    public override string ToString() => Value;

    public static implicit operator string(TicketDescription description) => description.Value;

    public override bool Equals(object? obj)
    {
        if (obj is not TicketDescription other) return false;
        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
