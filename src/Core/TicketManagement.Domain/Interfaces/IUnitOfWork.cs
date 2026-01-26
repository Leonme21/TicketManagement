﻿namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// Patrón Unit of Work:   agrupa múltiples operaciones en una transacción
/// Garantiza que todos los cambios se guardan juntos o ninguno se guarda
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    ITicketRepository Tickets { get; }
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    ITagRepository Tags { get; }

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos
    /// Retorna el número de entidades afectadas
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una transacción explícita (para operaciones complejas)
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma la transacción actual
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción actual
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
