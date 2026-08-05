using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Common.Caching;

public static class DependencyInjection
{
    public static IServiceCollection AddCommonCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();

        var redisConnection = configuration.GetConnectionString("Redis");
        var useRedis = !string.IsNullOrWhiteSpace(redisConnection);

        if (useRedis)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "EventBride:";
            });
        }
        else
        {
            // بدون Redis، از IDistributedCache درون‌فرایندی استفاده کن
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, CacheService>(sp =>
            new CacheService(
                sp.GetRequiredService<IDistributedCache>(),
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CacheService>>(),
                useRedis));

        return services;
    }
}