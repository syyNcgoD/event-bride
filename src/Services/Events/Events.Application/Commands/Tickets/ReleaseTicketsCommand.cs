using Events.Application.Common.Models;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Commands.Tickets;

public record ReleaseTicketsCommand(int TicketTypeId, int Quantity) : IRequest<ApiResponse<object>>;

public class ReleaseTicketsCommandHandler : IRequestHandler<ReleaseTicketsCommand, ApiResponse<object>>
{
    private readonly ITicketTypeRepository _ticketTypeRepository;

    public ReleaseTicketsCommandHandler(ITicketTypeRepository ticketTypeRepository)
    {
        _ticketTypeRepository = ticketTypeRepository;
    }

    public async Task<ApiResponse<object>> Handle(ReleaseTicketsCommand request, CancellationToken cancellationToken)
    {
        var released = await _ticketTypeRepository.TryReleaseAsync(
            request.TicketTypeId, request.Quantity, cancellationToken);

        return released
            ? ApiResponse<object>.Ok(new { Released = true })
            : ApiResponse<object>.Fail("آزادسازی ناموفق بود");
    }
}