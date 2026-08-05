var builder = WebApplication.CreateBuilder(args);

// اضافه کردن تنظیمات YARP از فایل appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/", () => "EventBride API Gateway is running!");

// راه‌اندازی میان‌افزار YARP برای مسیریابی درخواست‌ها
app.MapReverseProxy();

app.Run();