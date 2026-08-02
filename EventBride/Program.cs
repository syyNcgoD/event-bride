var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "EventBride API is running!");

app.Run();
