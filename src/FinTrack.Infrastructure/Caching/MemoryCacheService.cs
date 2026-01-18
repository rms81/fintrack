using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace FinTrack.Infrastructure.Caching;

/// <summary>
/// In-memory cache implementation using IMemoryCache.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out T? cached))
        {
            return cached!;
        }

        // Acquire a lock for this specific key to prevent cache stampede
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);

        try
        {
            // Double-check after acquiring lock - another thread may have populated the cache
            if (_cache.TryGetValue(key, out cached))
            {
                return cached!;
            }

            var value = await factory(ct);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? DefaultExpiration
            };

            options.RegisterPostEvictionCallback((k, v, r, s) =>
            {
                _keys.TryRemove(k.ToString()!, out _);
            });

            _cache.Set(key, value, options);
            _keys.TryAdd(key, 0);

            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void Remove(string key)
    {
        _keys.TryRemove(key, out _);
        _cache.Remove(key);
        // Note: Lock cleanup is handled by PostEvictionCallback to avoid race conditions
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keysToRemove)
        {
            Remove(key);
        }
    }
}
