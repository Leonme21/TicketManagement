﻿﻿﻿using AutoMapper;
using MediatR;
using TicketManagement.Application.Common.Exceptions;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.AddComment;

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, CommentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public AddCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ITicketRepository ticketRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _unitOfWork = unitOfWork;
        _ticketRepository = ticketRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener Aggregate Root (Ticket)
        // PERFORMANCE: Obtenemos la entidad TRACKED para realizar modificaciones.
        // Se asume que GetByIdAsync no incluye colecciones pesadas (como el historial completo de comentarios).
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new NotFoundException(nameof(Ticket), request.TicketId);
        }

        // Obtener usuario autenticado
        var userId = _currentUserService.UserIdInt;
        if (!userId.HasValue)
        {
            throw new ForbiddenAccessException("User is not authenticated");
        }

        // SECURITY FIX: Validar autorización (IDOR)
        // Solo el creador del ticket o un Agente/Admin pueden comentar
        var userRole = await _userRepository.GetUserRoleAsync(userId.Value, cancellationToken);
        if (ticket.CreatorId != userId.Value && userRole != UserRole.Agent && userRole != UserRole.Admin)
        {
            throw new ForbiddenAccessException("You are not authorized to comment on this ticket.");
        }

        // 2. Ejecutar Lógica de Dominio
        // Delegamos la creación al Aggregate Root para mantener consistencia
        var comment = ticket.AddComment(request.Content, userId.Value);

        // 3. Persistir
        // Al estar trackeado el ticket y haber modificado su colección Comments,
        // SaveChanges detectará la nueva entidad y la insertará. No hace falta Update().
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Retornar DTO
        // El comentario ya tiene ID generado tras SaveChanges
        return _mapper.Map<CommentDto>(comment);
    }
}