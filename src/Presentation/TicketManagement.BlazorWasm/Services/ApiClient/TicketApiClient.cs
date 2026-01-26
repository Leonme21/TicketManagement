using System.Net.Http.Json;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Application.Contracts.Categories;
using TicketManagement.BlazorWasm.Models;

namespace TicketManagement.BlazorWasm.Services.ApiClient;

public class TicketApiClient : ITicketApiClient
{
    private readonly HttpClient _httpClient;

    public TicketApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginatedList<TicketDto>> GetTicketsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _httpClient.GetFromJsonAsync<PaginatedList<TicketDto>>($"api/Tickets?pageNumber={pageNumber}&pageSize={pageSize}")
            ?? new PaginatedList<TicketDto>();
    }

    public async Task<TicketDetailsDto> GetTicketByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<TicketDetailsDto>($"api/Tickets/{id}")
            ?? throw new Exception("Ticket not found");
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Tickets", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TicketDto>()
            ?? throw new Exception("Failed to create ticket");
    }

    public async Task UpdateTicketAsync(int id, UpdateTicketRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Tickets/{id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTicketAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/Tickets/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignTicketAsync(int id, int agentId)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Tickets/{id}/assign", new { ticketId = id, agentId });
        response.EnsureSuccessStatusCode();
    }

    public async Task CloseTicketAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/Tickets/{id}/close", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TicketDto>> GetMyTicketsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<TicketDto>>("api/Tickets/my-tickets")
            ?? new List<TicketDto>();
    }

    public async Task<List<TicketDto>> GetAssignedTicketsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<TicketDto>>("api/Tickets/assigned-to-me")
            ?? new List<TicketDto>();
    }

    public async Task AddCommentAsync(int ticketId, AddCommentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Tickets/{ticketId}/comments", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/Categories")
            ?? new List<CategoryDto>();
    }
}