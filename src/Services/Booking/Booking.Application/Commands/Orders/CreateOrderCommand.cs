using Booking.Application.Common.Models;
using Booking.Application.DTOs;
using Booking.Domain.Entities;
using Booking.Domain.Interfaces;
using MediatR;

namespace Booking.Application.Commands.Orders;

public record CreateOrderCommand(
    string UserId,
    string Email,
    string? PhoneNumber,
    string? Notes,
    List<OrderItemRequest> Items) : IRequest<ApiResponse<OrderResponse>>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResponse<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketInventoryService _inventory;

    // مدت زمان اعتبار رزرو موقت
    private static readonly TimeSpan ReservationTimeout = TimeSpan.FromMinutes(10);

    public CreateOrderCommandHandler(IOrderRepository orderRepository, ITicketInventoryService inventory)
    {
        _orderRepository = orderRepository;
        _inventory = inventory;
    }

    public async Task<ApiResponse<OrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // ۱) جمع‌آوری اطلاعات تیکت‌ها از Events Service
        var itemDetails = new List<(OrderItemRequest Req, TicketAvailability Availability)>();
        foreach (var item in request.Items)
        {
            var availability = await _inventory.GetTicketAvailabilityAsync(item.TicketTypeId, cancellationToken);
            if (availability is null)
            {
                return ApiResponse<OrderResponse>.Fail($"نوع بلیط {item.TicketTypeId} یافت نشد");
            }

            // ۲) بررسی موجودی
            if (item.Quantity > availability.AvailableQuantity)
            {
                return ApiResponse<OrderResponse>.Fail(
                    $"موجودی کافی برای بلیط «{availability.TicketTypeName}» وجود ندارد");
            }

            if (item.Quantity > availability.MaxPerOrder)
            {
                return ApiResponse<OrderResponse>.Fail(
                    $"تعداد درخواستی از حد مجاز {availability.MaxPerOrder} بیشتر است");
            }

            itemDetails.Add((item, availability));
        }

        // ۳) رزرو تیکت‌ها در Events Service (با قفل Pessimistic)
        foreach (var (req, availability) in itemDetails)
        {
            var reserved = await _inventory.ReserveTicketsAsync(
                req.TicketTypeId, availability.EventId, req.Quantity, cancellationToken);

            if (!reserved)
            {
                // رزرو ناموفق: همه چیز را برمی‌گردانیم
                await ReleaseReservedTicketsAsync(itemDetails, cancellationToken);
                return ApiResponse<OrderResponse>.Fail("رزرو ناموفق بود، موجودی تغییر کرده است");
            }
        }

        // ۴) ساخت سفارش با اقلام
        var order = new Order
        {
            UserId = request.UserId,
            OrderNumber = GenerateOrderNumber(),
            Status = OrderStatus.Pending,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Notes = request.Notes,
            ExpiresAt = DateTime.UtcNow.Add(ReservationTimeout)
        };

        foreach (var (req, availability) in itemDetails)
        {
            var totalPrice = req.Quantity * availability.UnitPrice;
            order.Items.Add(new OrderItem
            {
                TicketTypeId = req.TicketTypeId,
                EventId = availability.EventId,
                EventTitle = availability.EventTitle,
                TicketTypeName = availability.TicketTypeName,
                Quantity = req.Quantity,
                UnitPrice = availability.UnitPrice,
                TotalPrice = totalPrice
            });

            order.TotalAmount += totalPrice;
        }

        order.StatusHistory.Add(new OrderStatusHistory
        {
            NewStatus = OrderStatus.Pending,
            ChangedBy = request.UserId,
            Reason = "سفارش ایجاد شد"
        });

        await _orderRepository.AddAsync(order, cancellationToken);

        var created = await _orderRepository.GetByIdWithItemsAsync(order.Id, cancellationToken);
        if (created is null)
        {
            return ApiResponse<OrderResponse>.Fail("سفارش ساخته شد اما بازیابی ناموفق بود");
        }

        return ApiResponse<OrderResponse>.Ok(
            MapToResponse(created),
            "رزرو موقت انجام شد. لطفاً تا ۱۰ دقیقه پرداخت را تکمیل کنید.");
    }

    private async Task ReleaseReservedTicketsAsync(
        List<(OrderItemRequest Req, TicketAvailability Availability)> items,
        CancellationToken cancellationToken)
    {
        foreach (var (req, availability) in items)
        {
            await _inventory.ReleaseTicketsAsync(req.TicketTypeId, req.Quantity, cancellationToken);
        }
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..22];
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