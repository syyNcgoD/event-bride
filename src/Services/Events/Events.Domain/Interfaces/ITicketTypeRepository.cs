using Events.Domain.Entities;

namespace Events.Domain.Interfaces;

public interface ITicketTypeRepository
{
    Task<IReadOnlyList<TicketType>> GetByEventIdAsync(int eventId, CancellationToken cancellationToken = default);
    Task<TicketType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(TicketType ticketType, CancellationToken cancellationToken = default);
    Task UpdateAsync(TicketType ticketType);
    Task DeleteAsync(TicketType ticketType);

    /// <summary>
    /// رزرو اتمیک و Concurrency-safe: فقط اگر موجودی کافی و فروش فعال باشد کاهش می‌دهد.
    /// SQL Server قفل انحصاری (UPDLOCK) می‌گیرد.
    /// </summary>
    Task<bool> TryReserveAsync(int ticketTypeId, int eventId, int quantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// آزادسازی اتمیک: افزایش موجودی
    /// </summary>
    Task<bool> TryReleaseAsync(int ticketTypeId, int quantity, CancellationToken cancellationToken = default);
}