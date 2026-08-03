using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Queries.Events;

public record GetFeaturedEventsQuery(int Count = 5) : IRequest<ApiResponse<List<EventSummaryResponse>>>;

public class GetFeaturedEventsQueryHandler
    : IRequestHandler<GetFeaturedEventsQuery, ApiResponse<List<EventSummaryResponse>>>
{
    private readonly IEventRepository _eventRepository;

    public GetFeaturedEventsQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ApiResponse<List<EventSummaryResponse>>> Handle(
        GetFeaturedEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.GetFeaturedAsync(cancellationToken);

        var items = events
            .Take(Math.Max(request.Count, 1))
            .Select(e => new EventSummaryResponse(
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
                e.TicketTypes.Sum(tt => tt.AvailableQuantity)))
            .ToList();

        return ApiResponse<List<EventSummaryResponse>>.Ok(items);
    }
}
