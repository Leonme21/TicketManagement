using System;
using System.Collections.Concurrent;

namespace TicketManagement.Infrastructure.Caching;

public class CachePolicyRegistry : ICachePolicyRegistry
{
    private readonly ConcurrentDictionary<Type, TimeSpan> _policies = new();

    public void Register<T>(TimeSpan ttl)
    {
        _policies[typeof(T)] = ttl;
    }

    public TimeSpan? GetTtl<T>()
    {
        if (_policies.TryGetValue(typeof(T), out var ttl))
        {
            return ttl;
        }
        return null;
    }
}
