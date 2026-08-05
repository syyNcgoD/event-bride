using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Queries.Events;

public record GetEventByIdQuery(int Id) : IRequest<ApiResponse<EventResponse>>;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, ApiResponse<EventResponse>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventByIdQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ApiResponse<EventResponse>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (@event is null)
        {
            return ApiResponse<EventResponse>.Fail("رویداد یافت نشد");
        }

        var response = new EventResponse(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.ImageUrl,
            @event.VenueId,
            @event.Venue?.Name ?? string.Empty,
            @event.Venue?.City ?? string.Empty,
            @event.CategoryId,
            @event.Category?.Name ?? string.Empty,
            @event.OrganizerId,
            @event.StartDate,
            @event.EndDate,
            @event.DoorsOpen,
            @event.Status.ToString(),
            @event.IsFeatured,
            @event.IsUpcoming,
            @event.TicketTypes.Select(tt => new TicketTypeResponse(
                tt.Id, tt.EventId, tt.Event!.Title, tt.Name, tt.Description, tt.Price, tt.Quantity,
                tt.SoldCount, tt.AvailableQuantity, tt.MaxPerOrder,
                tt.SaleStart, tt.SaleEnd)).ToList());

        return ApiResponse<EventResponse>.Ok(response);
    }
}
