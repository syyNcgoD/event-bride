using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Queries.Events;

public record GetEventsQuery(
    int Page = 1,
    int PageSize = 20,
    bool UpcomingOnly = true) : IRequest<ApiResponse<PagedResult<EventSummaryResponse>>>;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, ApiResponse<PagedResult<EventSummaryResponse>>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventsQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ApiResponse<PagedResult<EventSummaryResponse>>> Handle(
        GetEventsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var events = request.UpcomingOnly
            ? await _eventRepository.GetUpcomingAsync(page, pageSize, cancellationToken)
            : await _eventRepository.GetUpcomingAsync(page, pageSize, cancellationToken);

        var totalCount = await _eventRepository.GetUpcomingCountAsync(cancellationToken);

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

        var paged = new PagedResult<EventSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ApiResponse<PagedResult<EventSummaryResponse>>.Ok(paged);
    }
}
