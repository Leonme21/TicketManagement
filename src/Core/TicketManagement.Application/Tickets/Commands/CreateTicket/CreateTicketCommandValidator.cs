using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Commands.CreateTicket;

/// <summary>
/// Validaciones para CreateTicketCommand
/// </summary>
public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be: Low, Medium, High, or Critical");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId is required");
    }


}
