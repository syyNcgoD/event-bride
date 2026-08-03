namespace Events.Domain.Entities;

public class TicketType
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int SoldCount { get; set; }
    public int MaxPerOrder { get; set; } = 10;
    public DateTime SaleStart { get; set; }
    public DateTime SaleEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }

    public int AvailableQuantity => Quantity - SoldCount;
    public bool IsOnSale => AvailableQuantity > 0 && SaleStart <= DateTime.UtcNow && DateTime.UtcNow <= SaleEnd;
}
