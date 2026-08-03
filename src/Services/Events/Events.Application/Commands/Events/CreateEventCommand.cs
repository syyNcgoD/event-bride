using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Domain.Entities;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Commands.Events;

public record CreateEventCommand(
    string Title,
    string? Description,
    string? ImageUrl,
    int VenueId,
    int CategoryId,
    string OrganizerId,
    DateTime StartDate,
    DateTime EndDate,
    DateTime? DoorsOpen,
    List<CreateTicketTypeRequest> TicketTypes) : IRequest<ApiResponse<EventResponse>>;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, ApiResponse<EventResponse>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ITicketTypeRepository _ticketTypeRepository;

    public CreateEventCommandHandler(
        IEventRepository eventRepository,
        ITicketTypeRepository ticketTypeRepository)
    {
        _eventRepository = eventRepository;
        _ticketTypeRepository = ticketTypeRepository;
    }

    public async Task<ApiResponse<EventResponse>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = new Event
        {
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            VenueId = request.VenueId,
            CategoryId = request.CategoryId,
            OrganizerId = request.OrganizerId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DoorsOpen = request.DoorsOpen,
            Status = EventStatus.Draft
        };

        await _eventRepository.AddAsync(@event, cancellationToken);

        foreach (var ticketType in request.TicketTypes)
        {
            await _ticketTypeRepository.AddAsync(new TicketType
            {
                EventId = @event.Id,
                Name = ticketType.Name,
                Description = ticketType.Description,
                Price = ticketType.Price,
                Quantity = ticketType.Quantity,
                MaxPerOrder = ticketType.MaxPerOrder,
                SaleStart = ticketType.SaleStart,
                SaleEnd = ticketType.SaleEnd
            }, cancellationToken);
        }

        var created = await _eventRepository.GetByIdWithDetailsAsync(@event.Id, cancellationToken);
        if (created is null)
        {
            return ApiResponse<EventResponse>.Fail("رویداد ساخته شد اما بازیابی ناموفق بود");
        }

        var response = MapToResponse(created);
        return ApiResponse<EventResponse>.Ok(response, "رویداد با موفقیت ایجاد شد");
    }

    private static EventResponse MapToResponse(Event @event)
    {
        return new EventResponse(
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
                tt.Id, tt.Name, tt.Description, tt.Price, tt.Quantity,
                tt.SoldCount, tt.AvailableQuantity, tt.MaxPerOrder,
                tt.SaleStart, tt.SaleEnd)).ToList());
    }
}
