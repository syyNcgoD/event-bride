using Events.Domain.Entities;

namespace Events.Domain.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetUpcomingAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetByOrganizerAsync(string organizerId, CancellationToken cancellationToken = default);
    Task AddAsync(Event @event, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event @event);
    Task DeleteAsync(Event @event);
    Task<int> GetUpcomingCountAsync(CancellationToken cancellationToken = default);
}
