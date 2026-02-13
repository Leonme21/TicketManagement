using FluentValidation;

namespace TicketManagement.Application.Tickets.Commands.UpdateTicket;

/// <summary>
/// Validaciones para UpdateTicketCommand
/// </summary>
public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0).WithMessage("TicketId is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be: Low, Medium, High, or Critical");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId is required");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control");
    }
}
