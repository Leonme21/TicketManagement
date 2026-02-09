using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tags.Commands;

/// <summary>
/// Handler para crear nuevo tag
/// ? REFACTORIZADO: Usa IUnitOfWork como �nico punto de acceso
/// </summary>
public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTagCommandHandler> _logger;

    public CreateTagCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateTagCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        // Validar que no exista un tag con el mismo nombre
        var existingTag = await _unitOfWork.Tags.GetByNameAsync(request.Name, cancellationToken);
        if (existingTag != null)
        {
            return Result<int>.Invalid($"Tag with name '{request.Name}' already exists");
        }

        // ? Factory Method con validaciones (Aprovechamos Result Pattern)
        var tagResult = Tag.Create(request.Name, request.Color);

        if (tagResult.IsFailure)
        {
            _logger.LogWarning("Domain validation failed for tag creation: {Error}", tagResult.Error);
            return Result<int>.Invalid(tagResult.Error);
        }

        var tag = tagResult.Value!;
        _unitOfWork.Tags.Add(tag);
        
        // ? UnitOfWork maneja el SaveChanges
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tag {TagId} created successfully with name {TagName}", tag.Id, tag.Name);

        return Result<int>.Success(tag.Id);
    }
}
