using MediatR;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Tags.Commands.AddTagToTicket;

/// <summary>
/// Command para agregar tag a un ticket
/// </summary>
public record AddTagToTicketCommand(int TicketId, int TagId) : IRequest<Result>;
