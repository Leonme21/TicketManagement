﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManagement.Domain.Common;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio base con operaciones CRUD genéricas
/// </summary>
public class BaseRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly IDateTime _dateTime;

    public BaseRepository(ApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _dateTime = dateTime;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<T?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);
    }

    public virtual void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public virtual void Update(T entity)
    {
        // Only attach if detached. If it's already tracked, changes are detected automatically.
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Update(entity);
        }
    }

    public virtual void Delete(T entity)
    {
        if (entity is ISoftDeletable softDeletable)
        {
            // Update soft delete flags
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = _dateTime.Now;

            // Ensure EF tracks these changes even if initially detached
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            
            entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified = true;
            entry.Property(nameof(ISoftDeletable.DeletedAt)).IsModified = true;
        }
        else
        {
            // Para Hard Delete, Remove funciona bien incluso si está Detached (EF lo adjunta para borrarlo)
            _dbSet.Remove(entity);
        }
    }
}