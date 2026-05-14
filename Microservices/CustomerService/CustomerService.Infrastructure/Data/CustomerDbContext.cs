using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Data;

/// <summary>
/// DbContext with only the Customer table — each microservice owns its own database.
/// Connection string points to CustomerServiceDb (separate from PostServiceDb).
/// </summary>
public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customer { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.CustomerId);
            entity.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.HasIndex(c => c.Name)
                  .IsUnique();
        });
    }
}
