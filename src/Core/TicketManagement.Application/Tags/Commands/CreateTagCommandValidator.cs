using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace TicketManagement.Application.Tags.Commands.CreateTag;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(50);

        RuleFor(v => v.Color)
            .Matches("^#([A-Fa-f0-9]{6})$")
            .WithMessage("Debe ser un color Hexadecimal válido (ej: #FF0000)");
    }
}
