using TicketManagement.Domain.Interfaces;
using TicketManagement.Infrastructure.Persistence.Repositories;

namespace TicketManagement.Infrastructure.Persistence;

/// <summary>
/// Unit of Work Pattern - Coordina m�ltiples repositorios y SaveChanges
/// ? Un solo punto de guardado para transacciones
/// ? REFACTORIZADO: Inicializaci�n directa (no lazy) para mejor predictibilidad
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // ✅ Instanciar directamente (no lazy) - más predecible
    public ITicketRepository Tickets { get; }
    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }
    public ITagRepository Tags { get; }
    public IRefreshTokenRepository RefreshTokens { get; } // ✅ NUEVO

    public UnitOfWork(
        ApplicationDbContext context,
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _context = context;
        Tickets = ticketRepository;
        Users = userRepository;
        Categories = categoryRepository;
        Tags = tagRepository;
        RefreshTokens = refreshTokenRepository;
    }

    /// <summary>
    /// Guarda todos los cambios de forma transaccional
    /// Los interceptores (Audit, SoftDelete, DomainEvents) se ejecutan aqu�
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Inicia una transacci�n expl�cita (para operaciones complejas)
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        await _context.Database.BeginTransactionAsync(ct);
    }

    /// <summary>
    /// Confirma la transacci�n actual
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        await _context.Database.CommitTransactionAsync(ct);
    }

    /// <summary>
    /// Revierte la transacci�n actual
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        await _context.Database.RollbackTransactionAsync(ct);
    }

    /// <summary>
    /// ? NUEVO: Ejecuta una operaci�n dentro de una transacci�n expl�cita
    /// �til para operaciones complejas que requieren m�ltiples pasos
    /// </summary>
    public async Task ExecuteTransactionAsync(Func<Task> action, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await action();
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
