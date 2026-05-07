using Microsoft.EntityFrameworkCore;
using Product.Domain.Models.Entities;

namespace Product.Infrastructure.Data;

public class ProductDbContext : DbContext
{
    public DbSet<Product.Domain.Models.Entities.Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product.Domain.Models.Entities.Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Stock)
                .IsRequired();
        });
    }
}
