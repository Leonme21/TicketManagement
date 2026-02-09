using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
namespace TicketManagement.Application.Tags.Commands.AddTagToTicket;
public class AddTagToTicketValidator : AbstractValidator<AddTagToTicketCommand>
{
    public AddTagToTicketValidator()
    {
        RuleFor(v => v.TicketId).GreaterThan(0);
        RuleFor(v => v.TagId).GreaterThan(0);
    }
}
