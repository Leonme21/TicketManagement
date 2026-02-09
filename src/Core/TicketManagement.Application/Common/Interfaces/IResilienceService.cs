namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Servicio de resiliencia para operaciones críticas
/// Implementa circuit breaker, retry policies y timeout handling
/// </summary>
public interface IResilienceService
{
    /// <summary>
    /// Ejecuta una operación con políticas de resiliencia aplicadas
    /// </summary>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, string operationName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ejecuta una operación sin valor de retorno con políticas de resiliencia
    /// </summary>
    Task ExecuteAsync(Func<CancellationToken, Task> operation, string operationName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica el estado del circuit breaker para una operación específica
    /// </summary>
    bool IsCircuitBreakerOpen(string operationName);
}