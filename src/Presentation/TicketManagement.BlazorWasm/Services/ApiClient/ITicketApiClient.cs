using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Application.Contracts.Categories;
using TicketManagement.BlazorWasm.Models;

namespace TicketManagement.BlazorWasm.Services.ApiClient;

public interface ITicketApiClient
{
    Task<PaginatedList<TicketDto>> GetTicketsAsync(int pageNumber = 1, int pageSize = 10);
    Task<TicketDetailsDto> GetTicketByIdAsync(int id);
    Task<CreateTicketResponse> CreateTicketAsync(CreateTicketRequest request);
    Task UpdateTicketAsync(int id, UpdateTicketApiRequest request);
    Task DeleteTicketAsync(int id);
    Task AssignTicketAsync(int id, int agentId);
    Task CloseTicketAsync(int id);
    Task<List<TicketDto>> GetMyTicketsAsync();
    Task<List<TicketDto>> GetAssignedTicketsAsync();
    Task AddCommentAsync(int ticketId, AddCommentApiRequest request);
    Task<List<CategoryDto>> GetCategoriesAsync();
}
