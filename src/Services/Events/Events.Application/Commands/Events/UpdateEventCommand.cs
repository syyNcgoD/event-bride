using Common.Caching;
using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Domain.Entities;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Commands.Events;

public record UpdateEventCommand(
    int Id,
    string Title,
    string? Description,
    string? ImageUrl,
    int VenueId,
    int CategoryId,
    DateTime StartDate,
    DateTime EndDate,
    DateTime? DoorsOpen,
    EventStatus Status,
    bool IsFeatured) : IRequest<ApiResponse<EventResponse>>;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, ApiResponse<EventResponse>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cacheService;

    public UpdateEventCommandHandler(IEventRepository eventRepository, ICacheService cacheService)
    {
        _eventRepository = eventRepository;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<EventResponse>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (@event is null)
        {
            return ApiResponse<EventResponse>.Fail("رویداد یافت نشد");
        }

        @event.Title = request.Title;
        @event.Description = request.Description;
        @event.ImageUrl = request.ImageUrl;
        @event.VenueId = request.VenueId;
        @event.CategoryId = request.CategoryId;
        @event.StartDate = request.StartDate;
        @event.EndDate = request.EndDate;
        @event.DoorsOpen = request.DoorsOpen;
        @event.Status = request.Status;
        @event.IsFeatured = request.IsFeatured;
        @event.UpdatedAt = DateTime.UtcNow;

        await _eventRepository.UpdateAsync(@event);

        // کش رویدادها را پاک کن — چون لیست/جزئیات تغییر کرده
        await InvalidateEventCachesAsync(@event.Id, cancellationToken);

        var updated = await _eventRepository.GetByIdWithDetailsAsync(@event.Id, cancellationToken);
        if (updated is null)
        {
            return ApiResponse<EventResponse>.Fail("به‌روزرسانی انجام شد اما بازیابی ناموفق بود");
        }

        return ApiResponse<EventResponse>.Ok(MapToResponse(updated), "رویداد با موفقیت به‌روزرسانی شد");
    }

    private async Task InvalidateEventCachesAsync(int eventId, CancellationToken cancellationToken)
    {
        // پاک کردن همه کش‌های رویدادها — چون لیست و جزئیات ممکنه تغییر کرده باشن
        await _cacheService.RemoveByPatternAsync("EventBride:events:*", cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.EventById.Replace("{id}", eventId.ToString()), cancellationToken);
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
                tt.Id, tt.EventId, tt.Event!.Title, tt.Name, tt.Description, tt.Price, tt.Quantity,
                tt.SoldCount, tt.AvailableQuantity, tt.MaxPerOrder,
                tt.SaleStart, tt.SaleEnd)).ToList());
    }
}
