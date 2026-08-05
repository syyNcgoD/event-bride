using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Queries.Tickets;

public record GetTicketByIdQuery(int TicketTypeId) : IRequest<ApiResponse<TicketTypeResponse>>;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, ApiResponse<TicketTypeResponse>>
{
    private readonly ITicketTypeRepository _ticketTypeRepository;

    public GetTicketByIdQueryHandler(ITicketTypeRepository ticketTypeRepository)
    {
        _ticketTypeRepository = ticketTypeRepository;
    }

    public async Task<ApiResponse<TicketTypeResponse>> Handle(
        GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketTypeRepository.GetByIdAsync(request.TicketTypeId, cancellationToken);
        if (ticket is null)
        {
            return ApiResponse<TicketTypeResponse>.Fail("نوع بلیط یافت نشد");
        }

        var response = new TicketTypeResponse(
            ticket.Id,
            ticket.EventId,
            ticket.Event?.Title ?? string.Empty,
            ticket.Name,
            ticket.Description,
            ticket.Price,
            ticket.Quantity,
            ticket.SoldCount,
            ticket.AvailableQuantity,
            ticket.MaxPerOrder,
            ticket.SaleStart,
            ticket.SaleEnd);

        return ApiResponse<TicketTypeResponse>.Ok(response);
    }
}