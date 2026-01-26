using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Ticket>> GetPagedAsync(TicketFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResult<T>> GetProjectedPagedAsync<T>(TicketFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    void Add(Ticket ticket);
    void Update(Ticket ticket);
    void Delete(Ticket ticket);
}