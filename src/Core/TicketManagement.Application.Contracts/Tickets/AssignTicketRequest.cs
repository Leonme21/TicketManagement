using System;
using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// ?? BIG TECH LEVEL: Request DTO for assigning a ticket to an agent
/// </summary>
public record AssignTicketApiRequest
{
    [Required(ErrorMessage = "AgentId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AgentId must be greater than 0")]
    public required int AgentId { get; init; }
}

/// <summary>
/// Legacy class maintained for backward compatibility
/// </summary>
[Obsolete("Use AssignTicketApiRequest instead")]
public class AssignTicketRequest
{
    public int AgentId { get; set; }
}
