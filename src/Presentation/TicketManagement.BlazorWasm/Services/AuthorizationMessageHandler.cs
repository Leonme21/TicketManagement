using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace TicketManagement.BlazorWasm.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthorizationMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Obtener el token del localStorage
        var token = await _localStorage.GetItemAsStringAsync("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            // Remover comillas si las tiene
            token = token.Trim('"');

            // Agregar el token al header Authorization
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
