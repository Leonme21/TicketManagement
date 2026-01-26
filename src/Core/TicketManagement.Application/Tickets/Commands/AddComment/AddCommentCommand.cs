using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Tickets;

namespace TicketManagement.Application.Tickets.Commands.AddComment;

/// <summary>
/// Command para agregar comentario a ticket
/// </summary>
public record AddCommentCommand : IRequest<CommentDto>
{
    public int TicketId { get; init; }
    public string Content { get; init; } = string.Empty;
}
