using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;

namespace TicketManagement.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor que convierte eliminaciones físicas en soft deletes
/// ? Refactorizado: Logging estructurado, manejo de errores, métricas
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly ILogger<SoftDeleteInterceptor> _logger;

    public SoftDeleteInterceptor(
        ICurrentUserService currentUserService, 
        IDateTime dateTime,
        ILogger<SoftDeleteInterceptor> logger)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Convierte eliminaciones físicas en soft deletes con logging y manejo de errores
    /// </summary>
    private void UpdateEntities(DbContext? context)
    {
        if (context == null)
        {
            _logger.LogWarning("DbContext is null in SoftDeleteInterceptor");
            return;
        }

        var deletedEntries = context.ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        if (!deletedEntries.Any())
            return;

        var currentUserId = GetCurrentUserId();
        var utcNow = _dateTime.UtcNow;
        var processedCount = 0;

        foreach (var entry in deletedEntries)
        {
            try
            {
                ConvertToSoftDelete(entry, currentUserId, utcNow);
                processedCount++;
                
                // ? Logging estructurado por entidad
                _logger.LogInformation(
                    "Soft deleted {EntityType} with ID {EntityId} by user {UserId}",
                    entry.Entity.GetType().Name,
                    GetEntityId(entry),
                    currentUserId ?? "System");
            }
            catch (Exception ex)
            {
                // ? Log de error pero continúa con otras entidades
                _logger.LogError(ex, 
                    "Failed to soft delete {EntityType} with ID {EntityId}",
                    entry.Entity.GetType().Name,
                    GetEntityId(entry));
            }
        }

        if (processedCount > 0)
        {
            _logger.LogInformation(
                "Soft delete batch completed: {ProcessedCount} entities marked as deleted",
                processedCount);
        }
    }

    /// <summary>
    /// Convierte una entrada individual a soft delete
    /// </summary>
    private static void ConvertToSoftDelete(
        EntityEntry<ISoftDeletable> entry, 
        string? userId, 
        DateTime utcNow)
    {
        entry.State = EntityState.Modified;
        entry.Entity.IsDeleted = true;
        entry.Entity.DeletedAt = utcNow;
        entry.Entity.DeletedBy = userId;
    }

    /// <summary>
    /// Obtiene el ID del usuario actual con fallback robusto
    /// </summary>
    private string? GetCurrentUserId()
    {
        try
        {
            return _currentUserService.UserId ?? "System";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, 
                "Failed to get current user ID, using 'System' as fallback");
            return "System";
        }
    }

    /// <summary>
    /// Obtiene el ID de la entidad para logging
    /// </summary>
    private static object? GetEntityId(EntityEntry entry)
    {
        try
        {
            // Intenta obtener la propiedad Id (convención)
            var idProperty = entry.Properties
                .FirstOrDefault(p => p.Metadata.Name == "Id");
            
            return idProperty?.CurrentValue;
        }
        catch
        {
            return "Unknown";
        }
    }
}
