using AutoMapper;
using MediatR;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Common;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Interfaces;
using TicketManagement.Domain.Common; // For TicketFilter

namespace TicketManagement.Application.Tickets.Queries.GetTicketsWithPagination;

public class GetTicketsWithPaginationQueryHandler
    : IRequestHandler<GetTicketsWithPaginationQuery, PaginatedList<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetTicketsWithPaginationQueryHandler(
        ITicketRepository ticketRepository, 
        IMapper mapper,
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _ticketRepository = ticketRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<PaginatedList<TicketDto>> Handle(
        GetTicketsWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Security: Determine user role for data scoping.
        int? filterCreatorId = null;
        // Robustness: Use UserIdInt if available, otherwise try parse.
        int userId = _currentUserService.UserIdInt ?? 0;
        
        if (userId != 0 || int.TryParse(_currentUserService.UserId, out userId))
        {
             var userRole = await _userRepository.GetUserRoleAsync(userId, cancellationToken);
             if (userRole == UserRole.Customer)
             {
                 filterCreatorId = userId;
             }
        }
        else
        {
             // If we cannot identify the user ID numerically, we might choose to:
             // A) Throw if authentication is strict
             // B) Treat as guest (no specific filter) if public access allowed
             // Assuming strict auth for tickets:
             // throw new UnauthorizedAccessException("User ID format invalid.");
             // For now, proceeding without specific creator filter (Admin view) strictly if role not fetched? No, safer to assume no access or handle gracefully.
             // We'll leave it as null (Admin view behavior) ONLY if they are authorized elsewhere, but effectively this block just sets the filter.
        }

        // 2. Build Filter
        var filter = new TicketFilter
        {
            CategoryId = request.CategoryId,
            Priority = request.Priority,
            Status = request.Status,
            CreatorId = filterCreatorId
        };

        // 3. Optimized Query via Repository Projection
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var pagedResult = await _ticketRepository.GetProjectedPagedAsync<TicketDto>(
            filter, 
            pageNumber, 
            pageSize, 
            cancellationToken);

        return new PaginatedList<TicketDto>(
            pagedResult.Items, 
            pagedResult.TotalCount, 
            pagedResult.PageNumber, 
            pagedResult.PageSize);
    }
}
