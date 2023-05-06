using Microsoft.AspNetCore.Identity;
using RecomField.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace RecomField.Models;

[Index("Id", IsUnique = true)]
public class Review
{
    public int Id { get; set; }

    [Required]
    [MinLength(1)]
    public string AuthorId { get; set; }

    [Required]
    [ForeignKey("AuthorId")]
    public ApplicationUser Author { get; set; }

    public int ProductId { get; set; }

    [Required]
    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [Required]
    [MinLength(1)]
    public string Title { get; set; }

    [Required]
    [MinLength(10)]
    public string Body { get; set; }

    [Required]
    public Score<Review> Score { get; set; }

    [DataType(DataType.Date)]
    public DateTime PublicationDate { get; set; }

    public List<Tag<Review>> Tags { get; set; } = new();

    public List<Like<Review>> Likes { get; set; } = new();

    public List<Comment<Review>> Comments { get; set; } = new();

    public int LikeCounter { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; }

    public Review() { }

    public async Task LoadAsync(ApplicationDbContext context, string? userId, bool deep = false)
    {
        if (Author == null)
        {
            await context.Entry(this).Reference(r => r.Author).LoadAsync();
            if (Author == null) throw new Exception("Author is not found");
        }
        if (Product == null)
        {
            await context.Entry(this).Reference(r => r.Product).LoadAsync();
            if (Product == null) throw new Exception("Product is not found");
        }
        await Product.LoadAsync(context, userId);
        if (Score == null)
        {
            await context.Entry(this).Reference(u => u.Score).LoadAsync();
            if (Score == null) throw new Exception("Score is not found");
        }
        if (Tags.Count == 0) await context.Entry(this).Collection(u => u.Tags).LoadAsync();
        if (deep)
        {
            await context.Entry(this).Collection(u => u.Likes).LoadAsync();
            await context.ReviewComments.Where(k => k.Entity == this).Include(k => k.Sender).LoadAsync();
            LikeCounter = Likes.Count;
        }
        else await context.ReviewLikes.FirstOrDefaultAsync(l => l.SenderId == userId && l.EntityId == Id);
    }
}
