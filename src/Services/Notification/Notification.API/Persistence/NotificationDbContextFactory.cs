using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notification.API.Persistence;

public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=EventBride;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

        return new NotificationDbContext(optionsBuilder.Options);
    }
}