namespace FinTrack.Infrastructure.Caching;

/// <summary>
/// Service for caching data with support for typed entries and invalidation.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets an item from the cache, or creates it if it doesn't exist.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken ct = default);

    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Removes all items matching a pattern (for invalidation).
    /// </summary>
    void RemoveByPrefix(string prefix);
}
