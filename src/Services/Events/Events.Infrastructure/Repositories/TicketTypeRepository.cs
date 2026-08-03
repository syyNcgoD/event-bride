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
}
