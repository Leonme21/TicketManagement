using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Tags;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;
namespace TicketManagement.Application.Tags.Commands.CreateTag;
public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateTagCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear entidad de Dominio
        var tag = new Tag(request.Name, request.Color);
        // 2. Guardar en BD (Ahora sí funciona .Tags)
        _unitOfWork.Tags.Add(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // 3. Devolver DTO
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }
}
