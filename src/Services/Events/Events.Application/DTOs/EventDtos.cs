using Events.Domain.Entities;

namespace Events.Application.DTOs;

public class CreateEventRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int VenueId { get; set; }
    public int CategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? DoorsOpen { get; set; }
    public List<CreateTicketTypeRequest> TicketTypes { get; set; } = [];
}

public class UpdateEventRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int VenueId { get; set; }
    public int CategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? DoorsOpen { get; set; }
    public EventStatus Status { get; set; }
    public bool IsFeatured { get; set; }
}

public class CreateTicketTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int MaxPerOrder { get; set; } = 10;
    public DateTime SaleStart { get; set; }
    public DateTime SaleEnd { get; set; }
}

public record TicketTypeResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Quantity,
    int SoldCount,
    int AvailableQuantity,
    int MaxPerOrder,
    DateTime SaleStart,
    DateTime SaleEnd);

public record EventResponse(
    int Id,
    string Title,
    string? Description,
    string? ImageUrl,
    int VenueId,
    string VenueName,
    string VenueCity,
    int CategoryId,
    string CategoryName,
    string OrganizerId,
    DateTime StartDate,
    DateTime EndDate,
    DateTime? DoorsOpen,
    string Status,
    bool IsFeatured,
    bool IsUpcoming,
    List<TicketTypeResponse> TicketTypes);

public record EventSummaryResponse(
    int Id,
    string Title,
    string? ImageUrl,
    string VenueName,
    string VenueCity,
    string CategoryName,
    DateTime StartDate,
    string Status,
    bool IsFeatured,
    decimal MinTicketPrice,
    int AvailableTickets);