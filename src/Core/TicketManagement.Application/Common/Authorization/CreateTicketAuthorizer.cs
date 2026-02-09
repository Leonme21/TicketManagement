using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Tickets.Commands.CreateTicket;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Common.Authorization;

/// <summary>
/// Autorizador específico para creación de tickets
/// Valida permisos granulares basados en categoría y usuario
/// </summary>
public class CreateTicketAuthorizer : IAuthorizer<CreateTicketCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketAuthorizer(
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> AuthorizeAsync(CreateTicketCommand request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == 0)
            return Result.Unauthorized("User not authenticated");

        // Verificar si el usuario puede crear tickets en esta categoría
        var categoryAuthResult = await _authorizationService.CanCreateTicketInCategoryAsync(
            userId, request.CategoryId, cancellationToken);
        
        if (categoryAuthResult.IsFailure)
            return categoryAuthResult;

        // Verificar si la categoría existe y está activa
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
            return Result.NotFound($"Category {request.CategoryId} not found");

        // Aquí podrías agregar más validaciones específicas:
        // - Límites por departamento
        // - Horarios de creación
        // - Restricciones por tipo de usuario
        
        return Result.Success();
    }
}