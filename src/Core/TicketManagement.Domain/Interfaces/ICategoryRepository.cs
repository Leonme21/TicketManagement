﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// Contrato para acceso a datos de Categories
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<Category>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    void Add(Category category);

    void Update(Category category);

    void Delete(Category category);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
