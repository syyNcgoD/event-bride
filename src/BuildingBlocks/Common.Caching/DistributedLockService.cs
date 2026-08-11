using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Caching;

public class DistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<DistributedLockService> _logger;

    public DistributedLockService(ILogger<DistributedLockService> logger, IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    public async Task<IAsyncDisposable?> AcquireLockAsync(
        string resourceKey,
        TimeSpan expiration,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default)
    {
        if (_redis is null || !_redis.IsConnected)
        {
            _logger.LogWarning("Redis connection unavailable. Bypassing distributed lock for {ResourceKey}", resourceKey);
            return new NoOpLockHandle();
        }

        var db = _redis.GetDatabase();
        var lockValue = Guid.NewGuid().ToString("N");
        var lockKey = $"lock:{resourceKey}";
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < waitTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var acquired = await db.StringSetAsync(
                lockKey,
                lockValue,
                expiration,
                When.NotExists);

            if (acquired)
            {
                _logger.LogDebug("Acquired distributed lock for {ResourceKey}", resourceKey);
                return new RedisLockHandle(db, lockKey, lockValue, _logger);
            }

            await Task.Delay(50, cancellationToken);
        }

        _logger.LogWarning("Failed to acquire distributed lock for {ResourceKey} within {WaitTimeout}ms", resourceKey, waitTimeout.TotalMilliseconds);
        return null;
    }

    private sealed class RedisLockHandle : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _value;
        private readonly ILogger _logger;
        private int _disposed;

        public RedisLockHandle(IDatabase db, string key, string value, ILogger logger)
        {
            _db = db;
            _key = key;
            _value = value;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                try
                {
                    await _db.ScriptEvaluateAsync(
                        "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end",
                        new RedisKey[] { _key },
                        new RedisValue[] { _value });

                    _logger.LogDebug("Released distributed lock for {Key}", _key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error releasing distributed lock for {Key}", _key);
                }
            }
        }
    }

    private sealed class NoOpLockHandle : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
