using Events.Domain.Entities;
using Events.Domain.Interfaces;
using Events.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly EventsDbContext _dbContext;

    public EventRepository(EventsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // No-Tracking: مناسب برای خواندن (GetById)
    public async Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    // Tracking + Split Query: برای ویرایش + جلوگیری از Cartesian Explosion
    public async Task<Event?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .Include(e => e.TicketTypes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    // No-Tracking + No-Tracking برای لیست‌ها
    public async Task<IReadOnlyList<Event>> GetUpcomingAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Published && e.StartDate >= DateTime.UtcNow)
            .OrderByDescending(e => e.IsFeatured)
            .ThenBy(e => e.StartDate)
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .Include(e => e.TicketTypes)
            .AsSplitQuery()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Published
                        && e.IsFeatured
                        && e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .Include(e => e.Venue)
            .Include(e => e.Category)
            .Include(e => e.TicketTypes)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetByOrganizerAsync(
        string organizerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.CreatedAt)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await _dbContext.Events.AddAsync(@event, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Event @event)
    {
        _dbContext.Events.Update(@event);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Event @event)
    {
        _dbContext.Events.Remove(@event);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> GetUpcomingCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .AsNoTracking()
            .CountAsync(e => e.Status == EventStatus.Published && e.StartDate >= DateTime.UtcNow,
                cancellationToken);
    }
}
