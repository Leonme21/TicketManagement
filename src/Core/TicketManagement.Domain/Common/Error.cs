namespace TicketManagement.Domain.Common;

/// <summary>
/// 🔥 BIG TECH LEVEL: Comprehensive Error type for Result Pattern
/// Supports multiple error categories with proper HTTP status code mapping
/// </summary>
public readonly record struct Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    private Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    // === STATIC FACTORY METHODS ===
    public static Error None => new(string.Empty, string.Empty, ErrorType.None);
    
    public static Error NullValue => new("Error.NullValue", "Se proporcionó un valor nulo.", ErrorType.Validation);
    
    public static Error ConditionNotMet => new("Error.ConditionNotMet", "La condición especificada no se cumplió.", ErrorType.Validation);

    // === VALIDATION ERRORS (400 Bad Request) ===
    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    // === NOT FOUND ERRORS (404 Not Found) ===
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    // === CONFLICT ERRORS (409 Conflict) ===
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    // === FORBIDDEN ERRORS (403 Forbidden) ===
    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    // === UNAUTHORIZED ERRORS (401 Unauthorized) ===
    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    // === INTERNAL ERRORS (500 Internal Server Error) ===
    public static Error Internal(string code, string description) =>
        new(code, description, ErrorType.Internal);

    // === RATE LIMIT ERRORS (429 Too Many Requests) ===
    public static Error RateLimitExceeded(string code, string description) =>
        new(code, description, ErrorType.RateLimitExceeded);

    public static implicit operator string(Error error) => error.Code;
    
    public override string ToString() => $"{Code}: {Description}";
}

/// <summary>
/// 🔥 BIG TECH LEVEL: Error types that map to HTTP status codes
/// </summary>
public enum ErrorType
{
    None = 0,
    Validation = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    RateLimitExceeded = 429,
    Internal = 500
}
