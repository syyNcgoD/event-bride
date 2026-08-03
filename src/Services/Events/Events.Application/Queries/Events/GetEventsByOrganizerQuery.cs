using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Queries.Events;

public record GetEventsByOrganizerQuery(string OrganizerId) : IRequest<ApiResponse<List<EventSummaryResponse>>>;

public class GetEventsByOrganizerQueryHandler
    : IRequestHandler<GetEventsByOrganizerQuery, ApiResponse<List<EventSummaryResponse>>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventsByOrganizerQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ApiResponse<List<EventSummaryResponse>>> Handle(
        GetEventsByOrganizerQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.GetByOrganizerAsync(request.OrganizerId, cancellationToken);

        var items = events.Select(e => new EventSummaryResponse(
            e.Id,
            e.Title,
            e.ImageUrl,
            e.Venue?.Name ?? string.Empty,
            e.Venue?.City ?? string.Empty,
            e.Category?.Name ?? string.Empty,
            e.StartDate,
            e.Status.ToString(),
            e.IsFeatured,
            e.TicketTypes.Count > 0 ? e.TicketTypes.Min(tt => tt.Price) : 0,
            e.TicketTypes.Sum(tt => tt.AvailableQuantity))).ToList();

        return ApiResponse<List<EventSummaryResponse>>.Ok(items);
    }
}
