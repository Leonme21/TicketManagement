using FluentValidation;
using TicketManagement.Application.Tickets.Commands.CreateTicket;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Common.BusinessRules;

/// <summary>
/// Simple business rule validation integrated with FluentValidation
/// ? FIXED: Removed async validation of category - let handler verify existence/404
/// </summary>
public class CreateTicketBusinessRuleValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketBusinessRuleValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId is required");
        
        // ? FIXED: Removed async category existence validation
        // Let CreateTicketCommandHandler return NotFound(404) if category doesn't exist
        // This ensures proper HTTP status codes:
        // - 400 Bad Request for validation errors
        // - 404 Not Found for non-existent category
    }
}