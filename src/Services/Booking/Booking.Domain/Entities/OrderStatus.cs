namespace Booking.Domain.Entities;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    Expired = 4,
    Refunded = 5
}