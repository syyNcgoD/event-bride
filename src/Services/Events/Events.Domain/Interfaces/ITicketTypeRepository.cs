using Events.Domain.Entities;

namespace Events.Domain.Interfaces;

public interface ITicketTypeRepository
{
    Task<IReadOnlyList<TicketType>> GetByEventIdAsync(int eventId, CancellationToken cancellationToken = default);
    Task<TicketType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(TicketType ticketType, CancellationToken cancellationToken = default);
    Task UpdateAsync(TicketType ticketType);
    Task DeleteAsync(TicketType ticketType);
}
