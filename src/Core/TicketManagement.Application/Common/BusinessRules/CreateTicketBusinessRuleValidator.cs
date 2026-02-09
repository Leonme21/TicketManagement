using FluentValidation;
using TicketManagement.Application.Tickets.Commands.CreateTicket;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Common.BusinessRules;

/// <summary>
/// Simple business rule validation integrated with FluentValidation
/// </summary>
public class CreateTicketBusinessRuleValidator : AbstractValidator<CreateTicketCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketBusinessRuleValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.CategoryId)
            .MustAsync(CategoryExists).WithMessage("Category does not exist or is inactive");
    }

    private async Task<bool> CategoryExists(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
        return category != null && category.IsActive;
    }
}