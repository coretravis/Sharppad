using System;
using System.Collections.Generic;

namespace SharpPad.Client.Services.Caching;

public class ClientCache : IClientCache
{
    // Simple dictionary-based cache
    private readonly Dictionary<string, (object Value, DateTime Expiration)> _cache = new();

    public void Set<T>(string key, T item, TimeSpan expiration)
    {
        _cache[key] = (item!, DateTime.UtcNow.Add(expiration));
    }

    public bool TryGetValue<T>(string key, out T item)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow <= entry.Expiration)
            {
                item = (T)entry.Value;
                return true;
            }
            else
            {
                // Expired entry—remove it.
                _cache.Remove(key);
            }
        }
        item = default!;
        return false;
    }
}
