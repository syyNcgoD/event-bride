using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EventBus.RabbitMQ;

public static class DependencyInjection
{
    /// <summary>
    /// پیکربندی MassTransit و RabbitMQ برای سرویس‌ها
    /// </summary>
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? consumersAssembly = null)
    {
        var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqUser = configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPass = configuration["RabbitMQ:Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            // اگر اسمبلی Consumer داده شده بود، آن‌ها را ثبت کن (برای Notification Service)
            if (consumersAssembly != null)
            {
                x.AddConsumers(consumersAssembly);
            }

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost, "/", h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                if (consumersAssembly != null)
                {
                    cfg.ConfigureEndpoints(context);
                }
            });
        });

        return services;
    }
}