using Booking.Application.Common.Models;
using Booking.Application.DTOs;
using Booking.Domain.Interfaces;
using MediatR;

namespace Booking.Application.Queries.Orders;

public record GetOrderQuery(int OrderId, string UserId) : IRequest<ApiResponse<OrderResponse>>;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, ApiResponse<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResponse<OrderResponse>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
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

        var response = new OrderResponse(
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

        return ApiResponse<OrderResponse>.Ok(response);
    }
}