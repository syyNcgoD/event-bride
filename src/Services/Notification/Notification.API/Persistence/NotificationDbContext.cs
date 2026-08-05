using Microsoft.EntityFrameworkCore;
using Notification.API.Entities;

namespace Notification.API.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<NotificationLog> Notifications => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(n => n.Id);

            entity.Property(n => n.UserEmail).HasMaxLength(256).IsRequired();
            entity.Property(n => n.Subject).HasMaxLength(200).IsRequired();
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(n => n.UserEmail);
            entity.HasIndex(n => n.IsSent);
        });
    }
}