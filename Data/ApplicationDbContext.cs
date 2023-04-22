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

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Score<Product>> ProductScores { get; set; } = default!;
        public DbSet<Book> Books { get; set; } = default!;
        public DbSet<Movie> Movies { get; set; } = default!;
        public DbSet<Series> Series { get; set; } = default!;
        public DbSet<Game> Games { get; set; } = default!;

        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<Tag<Review>> ReviewTags { get; set; } = default!;
        public DbSet<Score<Review>> ReviewScores { get; set; } = default!;
        public DbSet<Like<Review>> ReviewLikes { get; set; } = default!;
        public DbSet<Comment<Review>> ReviewComments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Review>().HasOne(l => l.Product).WithMany(r => r.Reviews)
                .HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Tag<Review>>().HasOne(l => l.Entity).WithMany(r => r.Tags)
                .HasForeignKey(l => l.EntityId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Like<Review>>().HasOne(l => l.Entity).WithMany(r => r.Likes)
                .HasForeignKey(l => l.EntityId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Comment<Review>>().HasOne(l => l.Entity).WithMany(r => r.Comments)
                .HasForeignKey(l => l.EntityId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Score<Product>>().HasOne(l => l.Entity).WithMany(r => r.UserScores)
                .HasForeignKey(l => l.EntityId).OnDelete(DeleteBehavior.ClientCascade);
            builder.Entity<Score<Review>>().HasOne(l => l.Entity).WithOne(r => r.Score).OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}