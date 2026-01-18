namespace FinTrack.Infrastructure.Caching;

/// <summary>
/// Cache key constants and builders for consistent cache key generation.
/// </summary>
public static class CacheKeys
{
    private const string Prefix = "fintrack";

    /// <summary>
    /// Categories list for a profile.
    /// </summary>
    public static string Categories(Guid profileId) => $"{Prefix}:categories:{profileId}";

    /// <summary>
    /// Dashboard summary for a profile with date range.
    /// </summary>
    public static string DashboardSummary(Guid profileId, Guid? accountId, DateOnly from, DateOnly to)
        => $"{Prefix}:dashboard:summary:{profileId}:{accountId}:{from:yyyyMMdd}:{to:yyyyMMdd}";

    /// <summary>
    /// Category spending breakdown for a profile.
    /// </summary>
    public static string CategorySpending(Guid profileId, Guid? accountId, DateOnly from, DateOnly to)
        => $"{Prefix}:dashboard:category-spending:{profileId}:{accountId}:{from:yyyyMMdd}:{to:yyyyMMdd}";

    /// <summary>
    /// Top merchants for a profile.
    /// </summary>
    public static string TopMerchants(Guid profileId, Guid? accountId, DateOnly from, DateOnly to, int limit)
        => $"{Prefix}:dashboard:top-merchants:{profileId}:{accountId}:{from:yyyyMMdd}:{to:yyyyMMdd}:{limit}";

    /// <summary>
    /// Rules list for a profile.
    /// </summary>
    public static string Rules(Guid profileId) => $"{Prefix}:rules:{profileId}";

    /// <summary>
    /// Profile by ID.
    /// </summary>
    public static string Profile(Guid profileId) => $"{Prefix}:profile:{profileId}";

    /// <summary>
    /// Wildcard-based cache key pattern for a profile.
    /// </summary>
    /// <remarks>
    /// This pattern uses '*' wildcards and is not compatible with RemoveByPrefix, which relies on
    /// simple string prefix matching. It is currently unused and should not be used for cache
    /// invalidation. Use concrete cache key builders (e.g. Categories, Rules, Profile) instead.
    /// </remarks>
    [Obsolete("Wildcard pattern is not compatible with RemoveByPrefix and is not used. Do not use this method for cache invalidation.")]
    public static string ProfilePattern(Guid profileId) => $"{Prefix}:*:{profileId}:*";

    /// <summary>
    /// Get all dashboard cache key prefixes for a profile (for invalidation).
    /// </summary>
    /// <returns>Array of prefixes to pass to RemoveByPrefix</returns>
    public static string[] DashboardPrefixes(Guid profileId) =>
    [
        $"{Prefix}:dashboard:summary:{profileId}:",
        $"{Prefix}:dashboard:category-spending:{profileId}:",
        $"{Prefix}:dashboard:top-merchants:{profileId}:"
    ];
}
