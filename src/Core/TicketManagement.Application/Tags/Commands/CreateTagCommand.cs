using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Tags.Commands;

/// <summary>
/// Command para crear nuevo tag
/// </summary>
public record CreateTagCommand(string Name, string Color) : IRequest<Result<int>>;
