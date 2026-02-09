namespace TicketManagement.Domain.Services;

/// <summary>
/// ? STAFF LEVEL: Domain Service para lógica que no pertenece a una sola entidad
/// Centraliza reglas de negocio transversales
/// </summary>
public static class CategoryMappingService
{
    public static CategoryType MapToCategoryType(string categoryName)
    {
        return categoryName.ToLower() switch
        {
            "technical" or "tech" or "bug" => CategoryType.Technical,
            "billing" or "payment" or "invoice" => CategoryType.Billing,
            _ => CategoryType.General
        };
    }
}
