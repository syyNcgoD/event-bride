using Booking.Domain.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Booking.Application.BackgroundJobs;

public class ReservationCleanupJob
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketInventoryService _inventoryService;
    private readonly ILogger<ReservationCleanupJob> _logger;

    public ReservationCleanupJob(
        IOrderRepository orderRepository,
        ITicketInventoryService inventoryService,
        ILogger<ReservationCleanupJob> logger)
    {
        _orderRepository = orderRepository;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// این متد توسط Hangfire صدا زده می‌شود تا رزروهای منقضی شده را آزاد کند
    /// قفل توزیع‌شده Hangfire مانع اجرای همزمان در چند رپلیکا می‌شود
    /// </summary>
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ProcessExpiredReservationsAsync()
    {
        _logger.LogInformation("Starting expired reservations cleanup job...");

        // رزروهایی که تاریخ انقضای آنها گذشته است
        var expiredOrders = await _orderRepository.GetExpiringOrdersAsync(DateTime.UtcNow);

        if (!expiredOrders.Any())
        {
            _logger.LogInformation("No expired reservations found.");
            return;
        }

        _logger.LogInformation("Found {Count} expired reservations to process.", expiredOrders.Count);

        foreach (var order in expiredOrders)
        {
            try
            {
                // 1. آزادسازی بلیط‌ها در Events Service
                foreach (var item in order.Items)
                {
                    var released = await _inventoryService.ReleaseTicketsAsync(
                        item.TicketTypeId, item.Quantity);

                    if (!released)
                    {
                        _logger.LogWarning(
                            "Failed to release tickets for Order {OrderId}, TicketType {TicketTypeId}",
                            order.Id, item.TicketTypeId);
                    }
                }

                // 2. تغییر وضعیت سفارش به Expired
                order.Status = Domain.Entities.OrderStatus.Expired;
                order.UpdatedAt = DateTime.UtcNow;

                order.StatusHistory.Add(new Domain.Entities.OrderStatusHistory
                {
                    OldStatus = Domain.Entities.OrderStatus.Pending,
                    NewStatus = Domain.Entities.OrderStatus.Expired,
                    ChangedBy = "System_Hangfire",
                    Reason = "Expired due to timeout"
                });

                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation("Successfully expired order {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired order {OrderId}", order.Id);
            }
        }

        _logger.LogInformation("Expired reservations cleanup job completed.");
    }
}