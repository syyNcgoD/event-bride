namespace Common.Caching;

/// <summary>
/// سرویس کش یکپارچه با الگوی Cache-Aside
/// از Redis (Distributed) با fallback به In-Memory استفاده می‌کند
/// </summary>
public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

public static class CacheKeys
{
    public const string FeaturedEvents = "events:featured";
    public const string UpcomingEvents = "events:upcoming:{page}:{pageSize}";
    public const string EventById = "events:{id}";
}