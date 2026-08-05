using Booking.Domain.Entities;

namespace Booking.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetByUserCountAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order);
    Task<IReadOnlyList<Order>> GetExpiringOrdersAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}