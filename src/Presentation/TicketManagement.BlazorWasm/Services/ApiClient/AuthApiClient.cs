using System.Net.Http.Json;
using TicketManagement.Application.Contracts.Authentication;

namespace TicketManagement.BlazorWasm.Services.ApiClient;

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthenticationResult>()
            ?? throw new Exception("Login failed");
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthenticationResult>()
            ?? throw new Exception("Registration failed");
    }
}
