using Microsoft.EntityFrameworkCore;
using Customer.Domain.Models.Entities;

namespace Customer.Infrastructure.Data;

public class CustomerDbContext : DbContext
{
    public DbSet<Customer.Domain.Models.Entities.Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer.Domain.Models.Entities.Customer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.RegistrationDate)
                .IsRequired();
        });
    }
}
