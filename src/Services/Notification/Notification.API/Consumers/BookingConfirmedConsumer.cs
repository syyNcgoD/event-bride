using EventBus.RabbitMQ.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.API.Entities;
using Notification.API.Persistence;

namespace Notification.API.Consumers;

public class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
{
    private readonly ILogger<BookingConfirmedConsumer> _logger;
    private readonly NotificationDbContext _dbContext;

    public BookingConfirmedConsumer(ILogger<BookingConfirmedConsumer> logger, NotificationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Received BookingConfirmedEvent for Order: {OrderNumber}", @event.OrderNumber);

        // اینجا به جای ارسال ایمیل واقعی، در دیتابیس لاگ می‌کنیم
        var body = $"سلام، رزرو شما با موفقیت تایید شد.\n" +
                   $"رویداد: {@event.EventTitle}\n" +
                   $"تعداد بلیط: {@event.TotalTickets}\n" +
                   $"مبلغ کل: {@event.TotalAmount}\n" +
                   $"شماره پیگیری: {@event.OrderNumber}";

        var log = new NotificationLog
        {
            UserEmail = @event.UserEmail,
            Subject = $"تاییدیه خرید بلیط - {@event.EventTitle}",
            Body = body,
            IsSent = true,
            SentAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(log);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Confirmation email sent to {Email} for Order {OrderNumber}",
            @event.UserEmail, @event.OrderNumber);
    }
}