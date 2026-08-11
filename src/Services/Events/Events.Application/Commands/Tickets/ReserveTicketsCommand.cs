using Common.Caching;
using Events.Application.Common.Models;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Commands.Tickets;

public record ReserveTicketsCommand(int TicketTypeId, int EventId, int Quantity)
    : IRequest<ApiResponse<object>>;

public class ReserveTicketsCommandHandler : IRequestHandler<ReserveTicketsCommand, ApiResponse<object>>
{
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly IDistributedLockService? _lockService;

    public ReserveTicketsCommandHandler(
        ITicketTypeRepository ticketTypeRepository,
        IDistributedLockService? lockService = null)
    {
        _ticketTypeRepository = ticketTypeRepository;
        _lockService = lockService;
    }

    public async Task<ApiResponse<object>> Handle(ReserveTicketsCommand request, CancellationToken cancellationToken)
    {
        var resourceKey = $"ticket-reserve:{request.TicketTypeId}";

        await using var lockHandle = _lockService != null
            ? await _lockService.AcquireLockAsync(
                resourceKey,
                expiration: TimeSpan.FromSeconds(5),
                waitTimeout: TimeSpan.FromSeconds(3),
                cancellationToken)
            : null;

        // آپدیت اتمیک شرطی: فقط اگر موجودی کافی و فروش فعال باشد اعمال می‌شود
        // SQL Server در این UPDATE به صورت خودکار قفل انحصاری (UPDLOCK, ROWLOCK) می‌گیرد
        var reserved = await _ticketTypeRepository.TryReserveAsync(
            request.TicketTypeId,
            request.EventId,
            request.Quantity,
            cancellationToken);

        return reserved
            ? ApiResponse<object>.Ok(new { Reserved = true })
            : ApiResponse<object>.Fail("موجودی کافی نیست یا بلیط در حال فروش نیست");
    }
}