using Common.Logging;
using EventBus.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Notification.API.Consumers;
using Notification.API.Persistence;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// پیکربندی مشترک Serilog
builder.AddCommonSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(connectionString));

// اتصال به RabbitMQ و ثبت Consumerها
builder.Services.AddEventBus(builder.Configuration, Assembly.GetExecutingAssembly());

// برای بررسی وضعیت
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Notification" }));

// ایجاد دیتابیس در صورت نبود
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();