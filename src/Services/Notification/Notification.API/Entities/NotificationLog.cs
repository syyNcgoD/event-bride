namespace Notification.API.Entities;

public class NotificationLog
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}