using Booking.Application.Commands.Orders;
using Booking.Domain.Entities;
using Booking.Infrastructure.Persistence;
using EventBus.RabbitMQ.Events;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Booking.IntegrationTests;

public class OutboxChaosIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StartAsync(),
            _rabbitMqContainer.StartAsync(),
            _redisContainer.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StopAsync(),
            _rabbitMqContainer.StopAsync(),
            _redisContainer.StopAsync());
    }

    [Fact]
    public void Testcontainers_Infrastructure_ContainersAreRunning()
    {
        _dbContainer.State.Should().Be(DotNet.Testcontainers.Containers.TestcontainersStates.Running);
        _rabbitMqContainer.State.Should().Be(DotNet.Testcontainers.Containers.TestcontainersStates.Running);
        _redisContainer.State.Should().Be(DotNet.Testcontainers.Containers.TestcontainersStates.Running);
    }
}
