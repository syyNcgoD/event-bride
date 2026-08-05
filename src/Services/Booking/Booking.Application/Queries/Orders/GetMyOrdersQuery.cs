using Booking.Application.Common.Models;
using Booking.Application.DTOs;
using Booking.Domain.Interfaces;
using MediatR;

namespace Booking.Application.Queries.Orders;

public record GetMyOrdersQuery(string UserId, int Page = 1, int PageSize = 20)
    : IRequest<ApiResponse<PagedResult<OrderResponse>>>;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, ApiResponse<PagedResult<OrderResponse>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetMyOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResponse<PagedResult<OrderResponse>>> Handle(
        GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var orders = await _orderRepository.GetByUserAsync(
            request.UserId, page, pageSize, cancellationToken);
        var totalCount = await _orderRepository.GetByUserCountAsync(request.UserId, cancellationToken);

        var items = orders.Select(o => new OrderResponse(
            o.Id,
            o.OrderNumber,
            o.UserId,
            o.Status.ToString(),
            o.TotalAmount,
            o.Currency,
            o.Email,
            o.PhoneNumber,
            o.CreatedAt,
            o.ExpiresAt,
            o.Items.Select(i => new OrderItemResponse(
                i.Id, i.TicketTypeId, i.EventId, i.EventTitle, i.TicketTypeName,
                i.SeatNumber, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList(),
            o.Payments.Select(p => new PaymentResponse(
                p.Id, p.PaymentMethod, p.TransactionId, p.Amount,
                p.Status.ToString(), p.PaidAt)).ToList())).ToList();

        var paged = new PagedResult<OrderResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ApiResponse<PagedResult<OrderResponse>>.Ok(paged);
    }
}