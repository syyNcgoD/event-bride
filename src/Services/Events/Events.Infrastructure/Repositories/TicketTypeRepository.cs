using Events.Domain.Entities;
using Events.Domain.Interfaces;
using Events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Repositories;

public class TicketTypeRepository : ITicketTypeRepository
{
    private readonly EventsDbContext _dbContext;

    public TicketTypeRepository(EventsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TicketType>> GetByEventIdAsync(
        int eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TicketTypes
            .AsNoTracking()
            .Where(tt => tt.EventId == eventId)
            .OrderBy(tt => tt.Price)
            .ToListAsync(cancellationToken);
    }

    public async Task<TicketType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TicketTypes
            .AsNoTracking()
            .Include(tt => tt.Event)
            .FirstOrDefaultAsync(tt => tt.Id == id, cancellationToken);
    }

    public async Task AddAsync(TicketType ticketType, CancellationToken cancellationToken = default)
    {
        await _dbContext.TicketTypes.AddAsync(ticketType, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TicketType ticketType)
    {
        _dbContext.TicketTypes.Update(ticketType);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(TicketType ticketType)
    {
        _dbContext.TicketTypes.Remove(ticketType);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// رزرو اتمیک با UPDATE شرطی:
    /// - فقط اگر موجودی کافی باشد (SoldCount + qty <= Quantity) و فروش فعال باشد
    /// - SQL Server در UPDATE به صورت خودکار قفل انحصاری می‌گیرد
    /// - دو رزرو همزمان: یکی موفق می‌شود، دیگری صفر ردیف (rows affected = 0)
    /// </summary>
    public async Task<bool> TryReserveAsync(
        int ticketTypeId, int eventId, int quantity, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(
            """
            UPDATE [TicketTypes]
            SET [SoldCount] = [SoldCount] + {0},
                [UpdatedAt] = GETUTCDATE()
            WHERE [Id] = {1}
              AND [EventId] = {2}
              AND [SoldCount] + {0} <= [Quantity]
              AND [SaleStart] <= {3}
              AND [SaleEnd] >= {3}
            """,
            quantity, ticketTypeId, eventId, now);

        return rowsAffected > 0;
    }

    /// <summary>
    /// آزادسازی اتمیک: افزایش موجودی (برعکس رزرو)
    /// </summary>
    public async Task<bool> TryReleaseAsync(
        int ticketTypeId, int quantity, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(
            """
            UPDATE [TicketTypes]
            SET [SoldCount] = [SoldCount] - {0},
                [UpdatedAt] = GETUTCDATE()
            WHERE [Id] = {1}
              AND [SoldCount] - {0} >= 0
            """,
            quantity, ticketTypeId);

        return rowsAffected > 0;
    }
}