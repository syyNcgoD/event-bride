using Common.Caching;
using Events.Application.Commands.Tickets;
using Events.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Events.UnitTests;

public class ReserveTicketsCommandConcurrencyTests
{
    [Fact]
    public async Task Handle_WithDistributedLock_AcquiresLockAndReservesTicketsSuccessfully()
    {
        // Arrange
        var ticketTypeId = 10;
        var eventId = 1;
        var quantity = 2;

        var repositoryMock = new Mock<ITicketTypeRepository>();
        repositoryMock
            .Setup(r => r.TryReserveAsync(ticketTypeId, eventId, quantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var lockServiceMock = new Mock<IDistributedLockService>();
        var lockHandleMock = new Mock<IAsyncDisposable>();
        lockHandleMock.Setup(l => l.DisposeAsync()).Returns(ValueTask.CompletedTask);

        lockServiceMock
            .Setup(l => l.AcquireLockAsync(
                $"ticket-reserve:{ticketTypeId}",
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockHandleMock.Object);

        var handler = new ReserveTicketsCommandHandler(repositoryMock.Object, lockServiceMock.Object);
        var command = new ReserveTicketsCommand(ticketTypeId, eventId, quantity);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        repositoryMock.Verify(r => r.TryReserveAsync(ticketTypeId, eventId, quantity, It.IsAny<CancellationToken>()), Times.Once);
        lockServiceMock.Verify(l => l.AcquireLockAsync($"ticket-reserve:{ticketTypeId}", It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        lockHandleMock.Verify(l => l.DisposeAsync(), Times.Once);
    }
}
