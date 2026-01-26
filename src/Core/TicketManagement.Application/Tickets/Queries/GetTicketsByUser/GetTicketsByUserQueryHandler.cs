using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TicketManagement.Application.Common.Exceptions;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Queries.GetTicketsByUser;

    public class GetTicketsByUserQueryHandler : IRequestHandler<GetTicketsByUserQuery, TicketManagement.Application.Contracts.Common.PaginatedList<TicketDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetTicketsByUserQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<TicketManagement.Application.Contracts.Common.PaginatedList<TicketDto>> Handle(GetTicketsByUserQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserIdInt;
            if (!userId.HasValue)
            {
                throw new ForbiddenAccessException("User is not authenticated");
            }

            var filter = new TicketManagement.Domain.Common.TicketFilter { CreatorId = userId.Value };
            
            // Use translation to DTO directly in the database (Projection)
            var result = await _unitOfWork.Tickets.GetProjectedPagedAsync<TicketDto>(filter, request.PageNumber, request.PageSize, cancellationToken);
            
            // Create Application Layer PaginatedList from Domain PaginatedResult
            return new TicketManagement.Application.Contracts.Common.PaginatedList<TicketDto>(
                result.Items, 
                result.TotalCount, 
                result.PageNumber, 
                result.PageSize);
        }
    }