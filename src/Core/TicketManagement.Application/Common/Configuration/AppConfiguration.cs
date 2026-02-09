namespace TicketManagement.Application.Common.Configuration;

/// <summary>
/// ✅ SIMPLIFIED: Essential application configuration
/// Removed over-engineering, kept only what's needed
/// </summary>
public class AppConfiguration
{
    public required DatabaseConfiguration Database { get; init; }
    public required JwtConfiguration Jwt { get; init; }
    public required CacheConfiguration Cache { get; init; }
}

public class DatabaseConfiguration
{
    public required string ConnectionString { get; init; }
    public int CommandTimeout { get; init; } = 30;
    public int MaxRetryCount { get; init; } = 3;
}

public class JwtConfiguration
{
    public required string Secret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpirationMinutes { get; init; } = 60;
}

public class CacheConfiguration
{
    public string? RedisConnectionString { get; init; }
    public int DefaultExpirationMinutes { get; init; } = 30;
    public bool EnableDistributedCache { get; init; } = false;
}