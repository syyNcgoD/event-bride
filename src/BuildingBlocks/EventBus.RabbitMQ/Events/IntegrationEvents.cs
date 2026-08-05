namespace EventBus.RabbitMQ.Events;

public interface IntegrationEvent
{
    Guid Id { get; }
    DateTime CreationDate { get; }
}

public record BookingConfirmedEvent : IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreationDate { get; init; } = DateTime.UtcNow;

    public int OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string? UserPhone { get; init; }
    public decimal TotalAmount { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public int TotalTickets { get; init; }
}