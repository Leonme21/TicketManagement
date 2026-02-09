using System;

namespace TicketManagement.Infrastructure.Caching;

public interface ICachePolicyRegistry
{
    TimeSpan? GetTtl<T>();
    void Register<T>(TimeSpan ttl);
}
