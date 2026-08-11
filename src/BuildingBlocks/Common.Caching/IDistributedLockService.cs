namespace Common.Caching;

public interface IDistributedLockService
{
    /// <summary>
    /// اخذ قفل توزیع‌شده با الگوی RedLock / Redis Distributed Lock
    /// </summary>
    Task<IAsyncDisposable?> AcquireLockAsync(
        string resourceKey,
        TimeSpan expiration,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default);
}
