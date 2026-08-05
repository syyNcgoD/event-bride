using Booking.Domain.Entities;
using Booking.Domain.Interfaces;
using Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly BookingDbContext _dbContext;

    public OrderRepository(BookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    // Tracking برای عملیات ویرایش + Split Query برای آیتم‌ها
    public async Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Include(o => o.StatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserAsync(
        string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .AsSplitQuery()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetByUserCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .CountAsync(o => o.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order)
    {
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync();
    }

    // برای Hangfire: پیدا کردن رزروهای منقضی‌شده
    public async Task<IReadOnlyList<Order>> GetExpiringOrdersAsync(
        DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Pending
                        && o.ExpiresAt != null
                        && o.ExpiresAt < olderThan)
            .Include(o => o.Items)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }
}