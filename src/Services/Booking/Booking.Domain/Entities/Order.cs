namespace Booking.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "IRR";
    public string? Notes { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    // نسخه برای Optimistic Concurrency (RowVersion در دیتابیس)
    public byte[] RowVersion { get; set; } = [];

    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];

    public bool IsPending => Status == OrderStatus.Pending;
    public bool IsConfirmed => Status == OrderStatus.Confirmed;
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int TicketTypeId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string TicketTypeName { get; set; } = string.Empty;
    public string? SeatNumber { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // نسخه برای Optimistic Concurrency
    public byte[] RowVersion { get; set; } = [];

    public Order? Order { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRR";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order? Order { get; set; }
}

public enum PaymentStatus
{
    Pending = 1,
    Success = 2,
    Failed = 3,
    Refunded = 4
}

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public OrderStatus? OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order? Order { get; set; }
}
