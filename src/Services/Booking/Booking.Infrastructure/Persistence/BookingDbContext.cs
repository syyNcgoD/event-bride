using Booking.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // جداول Transactional Outbox — تضمین تحویل پیام حتی با crash
        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();

        builder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);

            entity.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(o => o.UserId).HasMaxLength(450).IsRequired();
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(o => o.Currency).HasMaxLength(3).HasDefaultValue("IRR");
            entity.Property(o => o.Notes).HasMaxLength(1000);
            entity.Property(o => o.Email).HasMaxLength(256).IsRequired();
            entity.Property(o => o.PhoneNumber).HasMaxLength(20);
            entity.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(o => o.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Optimistic Concurrency با RowVersion
            entity.Property(o => o.RowVersion).IsRowVersion();

            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.CreatedAt);
            entity.HasIndex(o => o.ExpiresAt);
            entity.HasIndex(o => new { o.UserId, o.Status });
            entity.HasIndex(o => new { o.Status, o.CreatedAt });

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(o => o.StatusHistory)
                .WithOne(h => h.Order)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.EventTitle).HasMaxLength(200).IsRequired();
            entity.Property(i => i.TicketTypeName).HasMaxLength(100).IsRequired();
            entity.Property(i => i.SeatNumber).HasMaxLength(20);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
            entity.Property(i => i.TotalPrice).HasPrecision(18, 2).IsRequired();

            entity.Property(i => i.RowVersion).IsRowVersion();

            entity.HasIndex(i => i.OrderId);
            entity.HasIndex(i => i.TicketTypeId);
            entity.HasIndex(i => i.EventId);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(p => p.TransactionId).HasMaxLength(100);
            entity.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("IRR");
            entity.Property(p => p.FailureReason).HasMaxLength(500);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(p => p.OrderId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.TransactionId);
            entity.HasIndex(p => p.PaidAt);
        });

        builder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("OrderStatusHistory");
            entity.HasKey(h => h.Id);

            entity.Property(h => h.ChangedBy).HasMaxLength(450);
            entity.Property(h => h.Reason).HasMaxLength(500);
            entity.Property(h => h.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(h => h.OrderId);
            entity.HasIndex(h => h.CreatedAt);
        });
    }
}