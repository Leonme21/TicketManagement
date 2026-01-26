using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Tags;

namespace TicketManagement.Application.Tags.Commands.CreateTag;

public record CreateTagCommand(string Name, string Color) : IRequest<TagDto>;
