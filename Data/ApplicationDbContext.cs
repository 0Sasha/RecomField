using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecomField.Models;

namespace RecomField.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tag> Tag { get; set; } = default!;

        public DbSet<Product> Product { get; set; } = default!;
        public DbSet<Score<Product>> ProductScore { get; set; } = default!;

        public DbSet<Review> Review { get; set; } = default!;
        public DbSet<Score<Review>> ReviewScore { get; set; } = default!;
        public DbSet<Like<Review>> ReviewLike { get; set; } = default!;
        public DbSet<Comment<Review>> ReviewComment { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Like<Review>>().HasOne(l => l.Entity).WithMany(r => r.Likes)
                .HasForeignKey(l => l.EntityId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Comment<Review>>().HasOne(l => l.Entity).WithMany(r => r.Comments)
                .HasForeignKey(l => l.EntityId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Score<Product>>().HasOne(l => l.Entity).WithMany(r => r.UserScores)
                .HasForeignKey(l => l.EntityId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Review>().HasOne(l => l.Product).WithMany(r => r.Reviews)
                .HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Score<Review>>().HasOne(l => l.Entity).WithOne(r => r.Score).OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}