using Booking.Domain.Entities;

namespace Booking.Application.DTOs;

public class CreateOrderRequest
{
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Notes { get; set; }
    public List<OrderItemRequest> Items { get; set; } = [];
}

public class OrderItemRequest
{
    public int TicketTypeId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class ConfirmPaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}

public record OrderItemResponse(
    int Id,
    int TicketTypeId,
    int EventId,
    string EventTitle,
    string TicketTypeName,
    string? SeatNumber,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public record PaymentResponse(
    int Id,
    string PaymentMethod,
    string? TransactionId,
    decimal Amount,
    string Status,
    DateTime? PaidAt);

public record OrderResponse(
    int Id,
    string OrderNumber,
    string UserId,
    string Status,
    decimal TotalAmount,
    string Currency,
    string Email,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    List<OrderItemResponse> Items,
    List<PaymentResponse> Payments);
