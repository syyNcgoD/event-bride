using Booking.Domain.Interfaces;
using Booking.Infrastructure.Persistence;
using Booking.Infrastructure.Repositories;
using Booking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(BookingDbContext).Assembly.GetName().Name)));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddHttpClient<ITicketInventoryService, TicketInventoryService>();

        return services;
    }
}