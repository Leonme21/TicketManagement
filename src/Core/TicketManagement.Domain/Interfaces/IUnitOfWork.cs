namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// Unit of Work Pattern - Coordina repositorios y transacciones
/// ? Inversi�n de Dependencias: Definido en Domain, implementado en Infrastructure
/// ? REFACTORIZADO: Agregado ExecuteTransactionAsync para operaciones complejas
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositorios
    ITicketRepository Tickets { get; }
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    ITagRepository Tags { get; }
    IRefreshTokenRepository RefreshTokens { get; } // ✅ NUEVO

    // Operaciones transaccionales
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Ejecuta una acci�n dentro de una transacci�n expl�cita
    /// Hace commit si tiene �xito, rollback si falla
    /// </summary>
    Task ExecuteTransactionAsync(Func<Task> action, CancellationToken ct = default);
}
