using System.Transactions;
using Booking.Application.Common.Models;
using Booking.Application.DTOs;
using Booking.Domain.Entities;
using Booking.Domain.Interfaces;
using EventBus.RabbitMQ.Events;
using MassTransit;
using MediatR;

namespace Booking.Application.Commands.Orders;

public record ConfirmPaymentCommand(
    int OrderId,
    string UserId,
    string PaymentMethod,
    string? TransactionId) : IRequest<ApiResponse<OrderResponse>>;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, ApiResponse<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public ConfirmPaymentCommandHandler(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ApiResponse<OrderResponse>> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
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

        if (!order.IsPending)
        {
            return ApiResponse<OrderResponse>.Fail(
                $"سفارش در وضعیت {order.Status} است و قابل پرداخت نیست");
        }

        if (order.IsExpired)
        {
            // رزرو منقضی شده؛ باید لغو شود
            order.Status = OrderStatus.Expired;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OldStatus = OrderStatus.Pending,
                NewStatus = OrderStatus.Expired,
                ChangedBy = request.UserId,
                Reason = "رزرو منقضی شد"
            });
            await _orderRepository.UpdateAsync(order);

            return ApiResponse<OrderResponse>.Fail(
                "مهلت پرداخت تمام شده است. لطفاً دوباره رزرو کنید.", null);
        }

        // Transactional Outbox: ذخیره سفارش + نوشتن پیام در Outbox،
        // داخل یک تراکنش — اگه هرکدوم fail بشه، هیچ‌کدوم commit نمی‌شه.
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // ثبت پرداخت موفق
        order.Payments.Add(new Payment
        {
            OrderId = order.Id,
            PaymentMethod = request.PaymentMethod,
            TransactionId = request.TransactionId,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Status = PaymentStatus.Success,
            PaidAt = DateTime.UtcNow
        });

        // تأیید نهایی سفارش
        order.Status = OrderStatus.Confirmed;
        order.ExpiresAt = null;
        order.UpdatedAt = DateTime.UtcNow;

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OldStatus = OrderStatus.Pending,
            NewStatus = OrderStatus.Confirmed,
            ChangedBy = request.UserId,
            Reason = "پرداخت موفق انجام شد"
        });

        await _orderRepository.UpdateAsync(order);

        // انتشار رویداد برای Notification Service
        var eventTitle = order.Items.FirstOrDefault()?.EventTitle ?? "رویداد";
        var totalTickets = order.Items.Sum(i => i.Quantity);

        await _publishEndpoint.Publish(new BookingConfirmedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserEmail = order.Email,
            UserPhone = order.PhoneNumber,
            TotalAmount = order.TotalAmount,
            EventTitle = eventTitle,
            TotalTickets = totalTickets
        }, cancellationToken);

        // Commit تراکنش — هم سفارش ذخیره می‌شه هم پیام در Outbox
        scope.Complete();

        var updated = await _orderRepository.GetByIdWithItemsAsync(order.Id, cancellationToken);
        if (updated is null)
        {
            return ApiResponse<OrderResponse>.Fail("پرداخت انجام شد اما بازیابی ناموفق بود");
        }

        return ApiResponse<OrderResponse>.Ok(
            MapToResponse(updated), "پرداخت موفق و بلیط تأیید شد");
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