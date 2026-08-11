using System.Net;
using Booking.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Xunit;

namespace Booking.UnitTests;

public class TicketInventoryServiceResilienceTests
{
    [Fact]
    public async Task ReserveTicketsAsync_On500ServerError_Retries3TimesAndReturnsFalse()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var attempts = 0;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attempts++;
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Services:EventsApi"] = "http://localhost:5002"
            })
            .Build());

        services.AddHttpClient<TicketInventoryService>()
            .ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object)
            .AddResilienceHandler("test-resilience", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .HandleResult(r => r.StatusCode == HttpStatusCode.InternalServerError),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(10),
                    BackoffType = DelayBackoffType.Constant
                });
            });

        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetRequiredService<TicketInventoryService>();

        // Act
        var result = await service.ReserveTicketsAsync(ticketTypeId: 1, eventId: 10, quantity: 2);

        // Assert
        result.Should().BeFalse();
        // 1 initial attempt + 3 retries = 4 total calls
        attempts.Should().Be(4);
    }
}
