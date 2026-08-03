namespace Events.Domain.Entities;

public enum EventStatus
{
    Draft = 1,
    Published = 2,
    Cancelled = 3,
    Completed = 4
}

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int VenueId { get; set; }
    public int CategoryId { get; set; }
    public string OrganizerId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? DoorsOpen { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Venue? Venue { get; set; }
    public EventCategory? Category { get; set; }
    public ICollection<TicketType> TicketTypes { get; set; } = [];

    public bool IsPublished => Status == EventStatus.Published;
    public bool IsUpcoming => Status == EventStatus.Published && StartDate > DateTime.UtcNow;
}
