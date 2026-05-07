using Microsoft.EntityFrameworkCore;
using Order.Domain.Models.Entities;

namespace Order.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public DbSet<global::Order.Domain.Models.Entities.Order> Orders { get; set; }
    public DbSet<global::Order.Domain.Models.Entities.OrderItem> OrderItems { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar entidad Order
        modelBuilder.Entity<global::Order.Domain.Models.Entities.Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderDate)
                .IsRequired();

            entity.Property(e => e.CustomerId)
                .IsRequired();

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            // Relación con OrderItems
            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar entidad OrderItem
        modelBuilder.Entity<global::Order.Domain.Models.Entities.OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderId)
                .IsRequired();

            entity.Property(e => e.ProductId)
                .IsRequired();

            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.Subtotal)
                .HasPrecision(18, 2)
                .IsRequired();
        });
    }
}
