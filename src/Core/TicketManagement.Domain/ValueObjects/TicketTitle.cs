using System.Collections.Generic;
using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.ValueObjects;

public sealed record TicketTitle
{
    public string Value { get; }

    private TicketTitle(string value)
    {
        Value = value;
    }

    public static Result<TicketTitle> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<TicketTitle>.Invalid("Ticket title cannot be empty");

        title = title.Trim();

        if (title.Length > 200)
            return Result<TicketTitle>.Invalid("Ticket title must not exceed 200 characters");

        return Result<TicketTitle>.Success(new TicketTitle(title));
    }

    public override string ToString() => Value;

    public static implicit operator string(TicketTitle title) => title.Value;
}
