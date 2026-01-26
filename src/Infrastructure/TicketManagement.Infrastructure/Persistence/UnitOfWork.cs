﻿using Microsoft.EntityFrameworkCore.Storage;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Interfaces;
using TicketManagement.Infrastructure.Persistence.Repositories;

namespace TicketManagement.Infrastructure.Persistence;

/// <summary>
/// Patrón Unit of Work - agrupa múltiples operaciones en una transacción
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private readonly IDateTime _dateTime;
    private ITagRepository _tagRepository;

    public ITicketRepository Tickets { get; }
    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }

    public ITagRepository Tags => _tagRepository ??= new TagRepository(_context, _dateTime);

    public UnitOfWork(
        ApplicationDbContext context,
        IDateTime dateTime,
        ITagRepository tagRepository,
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository)
    {
        _context = context;
        _dateTime = dateTime;
        _tagRepository = tagRepository;
        Tickets = ticketRepository;
        Users = userRepository;
        Categories = categoryRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
        // No disponer _context aquí. 
        // Su ciclo de vida es gestionado por el contenedor de DI (Scoped).
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
        GC.SuppressFinalize(this);
    }
}
