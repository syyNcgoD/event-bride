using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Caching;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly bool _useDistributed;
    private readonly IConnectionMultiplexer? _redis;

    public CacheService(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        ILogger<CacheService> logger,
        IConnectionMultiplexer? redis = null,
        bool useDistributed = true)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _logger = logger;
        _useDistributed = useDistributed;
        _redis = redis;
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache HIT for {Key}", key);
            return cached;
        }

        _logger.LogDebug("Cache MISS for {Key}", key);
        var value = await factory();
        if (value is not null)
        {
            await SetAsync(key, value, expiry, cancellationToken);
        }

        return value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_useDistributed)
            {
                var bytes = await _distributedCache.GetAsync(key, cancellationToken);
                if (bytes is null)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(bytes);
            }

            return _memoryCache.TryGetValue<T>(key, out var value) ? value : default;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache GET failed for {Key}, falling back", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
            };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await _distributedCache.SetAsync(key, bytes, options, cancellationToken);

            _memoryCache.Set(key, value, options.AbsoluteExpirationRelativeToNow!.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache SET failed for {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _memoryCache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE failed for {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis is not null)
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern).ToArray();
                foreach (var key in keys)
                {
                    // StackExchangeRedisCache کلیدها را با InstanceName (مثل EventBride:) ذخیره می‌کند.
                    // RemoveAsync خودش InstanceName را دوباره اضافه می‌کند، پس باید کلید خام (بدون prefix) را بدهیم.
                    var keyString = key.ToString();
                    if (keyString.StartsWith("EventBride:", StringComparison.Ordinal))
                    {
                        keyString = keyString["EventBride:".Length..];
                    }
                    await _distributedCache.RemoveAsync(keyString, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE by pattern failed for {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _distributedCache.GetAsync(key, cancellationToken) is not null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache EXISTS failed for {Key}", key);
            return false;
        }
    }
}