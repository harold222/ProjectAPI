using Microsoft.EntityFrameworkCore;
using PostService.Domain.Entities;

namespace PostService.Infrastructure.Data;

/// <summary>
/// DbContext with Post + Category tables — PostService owns these two entities.
/// No Customer table here — CustomerId is just an integer referencing CustomerService.
/// </summary>
public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

    public DbSet<Post> Post { get; set; }
    public DbSet<Category> Category { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.CategoryName)
                  .IsRequired()
                  .HasMaxLength(200);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.PostId);
            entity.Property(p => p.Title)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(p => p.Body)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(p => p.CustomerId)
                  .IsRequired();

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Posts)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(p => p.CustomerId);
            entity.HasIndex(p => p.CategoryId);
        });
    }
}
