namespace Booking.Domain.Interfaces;

/// <summary>
/// سرویس دسترسی به موجودی بلیط (در Events Service از طریق gRPC/REST)
/// </summary>
public interface ITicketInventoryService
{
    /// <summary>
    /// بررسی موجودی و قیمت یک نوع بلیط
    /// </summary>
    Task<TicketAvailability?> GetTicketAvailabilityAsync(
        int ticketTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// رزرو تیکت (کاهش SoldCount) با قفل Pessimistic
    /// </summary>
    Task<bool> ReserveTicketsAsync(
        int ticketTypeId, int eventId, int quantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// برگرداندن تیکت‌ها (افزایش موجودی) وقتی رزرو منقضی می‌شود
    /// </summary>
    Task<bool> ReleaseTicketsAsync(
        int ticketTypeId, int quantity, CancellationToken cancellationToken = default);
}

public record TicketAvailability(
    int TicketTypeId,
    int EventId,
    string EventTitle,
    string TicketTypeName,
    decimal UnitPrice,
    int AvailableQuantity,
    int MaxPerOrder);