namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Servicio de cache distribuido con funcionalidades avanzadas
/// Proporciona caching con TTL, invalidación por patrones y serialización automática
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtiene un valor del cache
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Establece un valor en el cache con TTL opcional
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Obtiene o establece un valor usando una factory function
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remueve un valor específico del cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remueve múltiples valores usando un patrón
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una clave en el cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el tiempo de vida restante de una clave
    /// </summary>
    Task<TimeSpan?> GetTtlAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Establece múltiples valores en batch
    /// </summary>
    Task SetManyAsync<T>(Dictionary<string, T> values, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Obtiene múltiples valores en batch
    /// </summary>
    Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class;
}