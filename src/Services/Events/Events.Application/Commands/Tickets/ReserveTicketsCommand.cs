using Events.Application.Common.Models;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Commands.Tickets;

public record ReserveTicketsCommand(int TicketTypeId, int EventId, int Quantity)
    : IRequest<ApiResponse<object>>;

public class ReserveTicketsCommandHandler : IRequestHandler<ReserveTicketsCommand, ApiResponse<object>>
{
    private readonly ITicketTypeRepository _ticketTypeRepository;

    public ReserveTicketsCommandHandler(ITicketTypeRepository ticketTypeRepository)
    {
        _ticketTypeRepository = ticketTypeRepository;
    }

    public async Task<ApiResponse<object>> Handle(ReserveTicketsCommand request, CancellationToken cancellationToken)
    {
        // آپدیت اتمیک شرطی: فقط اگر موجودی کافی و فروش فعال باشد اعمال می‌شود
        // SQL Server در این UPDATE به صورت خودکار قفل انحصاری (UPDLOCK) می‌گیرد
        // بنابراین دو رزرو همزمان نمی‌توانند هر دو موفق شوند
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