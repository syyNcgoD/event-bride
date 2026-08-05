using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Common.Logging;

public static class SerilogExtensions
{
    /// <summary>
    /// پیکربندی مشترک Serilog برای همه سرویس‌ها:
    /// Console + Seq (در صورت تنظیم) + فایل
    /// </summary>
    public static WebApplicationBuilder AddCommonSerilog(this WebApplicationBuilder builder)
    {
        var seqUrl = builder.Configuration["Serilog:SeqUrl"];

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            var config = configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", builder.Environment.ApplicationName)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}");

            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                config.WriteTo.Seq(seqUrl);
            }
        });

        return builder;
    }
}