using MediatR;

namespace TicketManagement.Application.Tags.Commands.AddTagToTicket;

public record AddTagToTicketCommand(int TicketId, int TagId) : IRequest;
