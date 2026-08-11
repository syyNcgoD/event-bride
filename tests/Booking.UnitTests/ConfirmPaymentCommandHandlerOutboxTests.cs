using Booking.Application.Commands.Orders;
using Booking.Domain.Entities;
using Booking.Domain.Interfaces;
using EventBus.RabbitMQ.Events;
using FluentAssertions;
using MassTransit;
using Moq;
using Xunit;

namespace Booking.UnitTests;

public class ConfirmPaymentCommandHandlerOutboxTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ConfirmPaymentCommandHandler _handler;

    public ConfirmPaymentCommandHandlerOutboxTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new ConfirmPaymentCommandHandler(_orderRepositoryMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPendingOrder_ConfirmsOrderAndPublishesOutboxEvent()
    {
        // Arrange
        var userId = "user-123";
        var orderId = 42;
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-2026-0001",
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = 500000,
            Currency = "IRR",
            Email = "user@example.com",
            PhoneNumber = "09123456789",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = 1,
                    EventId = 10,
                    EventTitle = "Concert 2026",
                    TicketTypeId = 100,
                    TicketTypeName = "VIP",
                    Quantity = 2,
                    UnitPrice = 250000,
                    TotalPrice = 500000
                }
            }
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var command = new ConfirmPaymentCommand(orderId, userId, "ZarinPal", "TXN-999888");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be("Confirmed");

        // Verify Order status updated
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.Payments.Should().HaveCount(1);
        order.Payments.First().Status.Should().Be(PaymentStatus.Success);

        // Verify Transactional Outbox publish was invoked
        _publishEndpointMock.Verify(
            p => p.Publish(
                It.Is<BookingConfirmedEvent>(e =>
                    e.OrderId == orderId &&
                    e.OrderNumber == "ORD-2026-0001" &&
                    e.UserEmail == "user@example.com" &&
                    e.TotalTickets == 2 &&
                    e.TotalAmount == 500000),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExpiredOrder_FailsAndDoesNotPublishOutboxEvent()
    {
        // Arrange
        var userId = "user-123";
        var orderId = 43;
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-2026-0002",
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = 250000,
            Email = "expired@example.com",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5) // Expired 5 mins ago
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new ConfirmPaymentCommand(orderId, userId, "ZarinPal", "TXN-000");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("مهلت پرداخت تمام شده است");

        // Verify no event published to Outbox
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<BookingConfirmedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
