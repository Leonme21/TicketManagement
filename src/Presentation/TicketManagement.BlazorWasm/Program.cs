using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Http;
using TicketManagement.BlazorWasm;
using TicketManagement.BlazorWasm.Services;
using TicketManagement.BlazorWasm.Services.ApiClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Leer la URL de la API desde configuración
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5227";

// LocalStorage (debe ir ANTES de HttpClient)
builder.Services.AddBlazoredLocalStorage();

// Registrar el AuthorizationMessageHandler
builder.Services.AddScoped<AuthorizationMessageHandler>();

// HttpClient con typed clients (mejora testabilidad y configuración)
builder.Services.AddHttpClient<ITicketApiClient, TicketApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// HttpClient general con handler (para CustomAuthStateProvider)
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();

    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };

    return httpClient;
});

// Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>(provider =>
    (CustomAuthStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());

await builder.Build().RunAsync();