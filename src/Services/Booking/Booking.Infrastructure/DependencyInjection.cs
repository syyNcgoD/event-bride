using System.Net;
using Booking.Domain.Interfaces;
using Booking.Infrastructure.Persistence;
using Booking.Infrastructure.Repositories;
using Booking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Booking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(BookingDbContext).Assembly.GetName().Name)));

        services.AddScoped<IOrderRepository, OrderRepository>();

        // Polly v8 Enterprise Distributed Resilience Pipeline
        services.AddHttpClient<ITicketInventoryService, TicketInventoryService>()
            .AddResilienceHandler("events-service-resilience", builder =>
            {
                // 1. Total Request Timeout Strategy
                builder.AddTimeout(TimeSpan.FromSeconds(10));

                // 2. Retry Strategy with Exponential Backoff + Jitter
                builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutException>()
                        .HandleResult(r => r.StatusCode == HttpStatusCode.InternalServerError ||
                                           r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                           r.StatusCode == HttpStatusCode.RequestTimeout ||
                                           r.StatusCode == HttpStatusCode.GatewayTimeout),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(300),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    OnRetry = args =>
                    {
                        Console.WriteLine($"[Polly Retry] Attempt {args.AttemptNumber} after {args.RetryDelay.TotalMilliseconds:F0}ms due to {args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()}");
                        return ValueTask.CompletedTask;
                    }
                });

                // 3. Circuit Breaker Strategy
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    FailureRatio = 0.5,
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(15),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutException>()
                        .HandleResult(r => (int)r.StatusCode >= 500),
                    OnOpened = args =>
                    {
                        Console.WriteLine($"[Polly CircuitBreaker] Circuit OPENED for {args.BreakDuration.TotalSeconds}s.");
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        Console.WriteLine("[Polly CircuitBreaker] Circuit CLOSED - Upstream service healthy.");
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = args =>
                    {
                        Console.WriteLine("[Polly CircuitBreaker] Circuit HALF-OPEN - Testing upstream health.");
                        return ValueTask.CompletedTask;
                    }
                });

                // 4. Per-Attempt Timeout Strategy
                builder.AddTimeout(TimeSpan.FromSeconds(3));
            });

        return services;
    }
}