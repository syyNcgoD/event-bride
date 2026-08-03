namespace Events.Domain.Entities;

public class EventCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public EventCategory? Parent { get; set; }
    public ICollection<EventCategory> Children { get; set; } = [];
    public ICollection<Event> Events { get; set; } = [];
}
