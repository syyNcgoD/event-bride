using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Persistence;

public class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Venue>(entity =>
        {
            entity.ToTable("Venues");
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Name).HasMaxLength(200).IsRequired();
            entity.Property(v => v.Address).HasMaxLength(500).IsRequired();
            entity.Property(v => v.City).HasMaxLength(100).IsRequired();
            entity.Property(v => v.Country).HasMaxLength(100).IsRequired();
            entity.Property(v => v.Description).HasMaxLength(2000);
            entity.Property(v => v.ImageUrl).HasMaxLength(500);
            entity.Property(v => v.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(v => v.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(v => v.IsActive).HasDefaultValue(true);

            entity.HasIndex(v => v.City);
            entity.HasIndex(v => v.IsActive);

            entity.HasMany(v => v.Events)
                .WithOne(e => e.Venue)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EventCategory>(entity =>
        {
            entity.ToTable("EventCategories");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(c => c.Name).IsUnique();

            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(c => c.Events)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.OrganizerId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.VenueId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.OrganizerId);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsFeatured);
            entity.HasIndex(e => new { e.Status, e.StartDate });
            entity.HasIndex(e => new { e.OrganizerId, e.Status });

            entity.HasMany(e => e.TicketTypes)
                .WithOne(tt => tt.Event)
                .HasForeignKey(tt => tt.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TicketType>(entity =>
        {
            entity.ToTable("TicketTypes");
            entity.HasKey(tt => tt.Id);

            entity.Property(tt => tt.Name).HasMaxLength(100).IsRequired();
            entity.Property(tt => tt.Description).HasMaxLength(500);
            entity.Property(tt => tt.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(tt => tt.MaxPerOrder).HasDefaultValue(10);
            entity.Property(tt => tt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(tt => tt.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(tt => tt.EventId);
            entity.HasIndex(tt => new { tt.SaleStart, tt.SaleEnd });
            entity.HasIndex(tt => tt.Price);
        });
    }
}
