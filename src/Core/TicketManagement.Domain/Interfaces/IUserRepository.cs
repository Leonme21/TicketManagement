﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// Contrato para acceso a datos de Users
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene solo el rol del usuario para validaciones ligeras.
    /// </summary>
    Task<UserRole?> GetUserRoleAsync(int id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    Task<List<User>> GetActiveAgentsAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);

    void Add(User user);

    void Update(User user);

    void Delete(User user);
}
