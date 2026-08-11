using Common.Caching;
using EventBus.RabbitMQ.Events;
using MassTransit;

namespace Events.API.Consumers;

public class EventUpdatedConsumer : IConsumer<EventUpdatedIntegrationEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<EventUpdatedConsumer> _logger;

    public EventUpdatedConsumer(ICacheService cacheService, ILogger<EventUpdatedConsumer> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventUpdatedIntegrationEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing EventUpdatedIntegrationEvent for EventId={EventId}, Action={Action}", message.EventId, message.Action);

        // Invalidate specific event cache and featured/upcoming listings
        await _cacheService.RemoveAsync($"events:{message.EventId}", context.CancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.FeaturedEvents, context.CancellationToken);
        await _cacheService.RemoveByPatternAsync("events:upcoming:*", context.CancellationToken);

        _logger.LogInformation("Successfully flushed Redis cache keys for EventId={EventId}", message.EventId);
    }
}
