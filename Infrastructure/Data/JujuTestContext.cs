using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class JujuTestContext : DbContext
{
    public JujuTestContext(DbContextOptions<JujuTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Category { get; set; }
    public virtual DbSet<Customer> Customer { get; set; }
    public virtual DbSet<Logs> Logs { get; set; }
    public virtual DbSet<Post> Post { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Logs>(entity =>
        {
            entity.Property(e => e.TimeStamp).HasColumnType("datetime");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId);
            entity.Property(e => e.Body).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.CategoryId).IsRequired(false);

            entity.HasOne(e => e.Customer)
                  .WithMany(e => e.Posts)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                  .WithMany(e => e.Posts)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}