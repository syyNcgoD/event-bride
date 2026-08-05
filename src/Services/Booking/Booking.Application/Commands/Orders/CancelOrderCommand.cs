using Booking.Application.Common.Models;
using Booking.Application.DTOs;
using Booking.Domain.Entities;
using Booking.Domain.Interfaces;
using MediatR;

namespace Booking.Application.Commands.Orders;

public record CancelOrderCommand(int OrderId, string UserId) : IRequest<ApiResponse<OrderResponse>>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ApiResponse<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketInventoryService _inventory;

    public CancelOrderCommandHandler(IOrderRepository orderRepository, ITicketInventoryService inventory)
    {
        _orderRepository = orderRepository;
        _inventory = inventory;
    }

    public async Task<ApiResponse<OrderResponse>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return ApiResponse<OrderResponse>.Fail("سفارش یافت نشد");
        }

        if (order.UserId != request.UserId)
        {
            return ApiResponse<OrderResponse>.Fail("شما دسترسی به این سفارش ندارید");
        }

        if (order.IsConfirmed)
        {
            // برای تأیید شده باید Refund انجام شود (منطق refund ساده‌سازی شده)
            order.Status = OrderStatus.Refunded;
        }
        else if (order.IsPending)
        {
            order.Status = OrderStatus.Cancelled;
        }
        else
        {
            return ApiResponse<OrderResponse>.Fail($"سفارش در وضعیت {order.Status} است و قابل لغو نیست");
        }

        // برگرداندن تیکت‌ها به موجودی
        foreach (var item in order.Items)
        {
            await _inventory.ReleaseTicketsAsync(item.TicketTypeId, item.Quantity, cancellationToken);
        }

        order.UpdatedAt = DateTime.UtcNow;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            OldStatus = order.Status == OrderStatus.Refunded ? OrderStatus.Confirmed : OrderStatus.Pending,
            NewStatus = order.Status,
            ChangedBy = request.UserId,
            Reason = order.Status == OrderStatus.Refunded ? "بازپرداخت" : "لغو توسط کاربر"
        });

        await _orderRepository.UpdateAsync(order);

        var updated = await _orderRepository.GetByIdWithItemsAsync(order.Id, cancellationToken);
        if (updated is null)
        {
            return ApiResponse<OrderResponse>.Fail("لغو انجام شد اما بازیابی ناموفق بود");
        }

        return ApiResponse<OrderResponse>.Ok(MapToResponse(updated), "لغو سفارش با موفقیت انجام شد");
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.Status.ToString(),
            order.TotalAmount,
            order.Currency,
            order.Email,
            order.PhoneNumber,
            order.CreatedAt,
            order.ExpiresAt,
            order.Items.Select(i => new OrderItemResponse(
                i.Id, i.TicketTypeId, i.EventId, i.EventTitle, i.TicketTypeName,
                i.SeatNumber, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList(),
            order.Payments.Select(p => new PaymentResponse(
                p.Id, p.PaymentMethod, p.TransactionId, p.Amount,
                p.Status.ToString(), p.PaidAt)).ToList());
    }
}