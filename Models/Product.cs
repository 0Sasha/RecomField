using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using System.ComponentModel.DataAnnotations;
namespace RecomField.Models;

[Index("Id", IsUnique = true)]
public abstract class Product
{
    private int relYear = 2023;

    public int Id { get; set; }

    [Required]
    [MinLength(1)]
    public virtual string? Title { get; set; }

    public virtual int ReleaseYear
    {
        get => relYear;
        set => relYear = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(ReleaseYear));
    }

    public virtual string Description { get; set; } = "";

    public virtual string? Cover { get; set; }

    public virtual double AverageUserScore { get; set; }

    public virtual double AverageReviewScore { get; set; }

    public virtual List<Score<Product>> UserScores { get; set; } = new();

    public virtual List<Review> Reviews { get; set; } = new();

    public Product() { }

    public virtual async Task LoadAsync(ApplicationDbContext context)
    {
        await context.Entry(this).Collection(p => p.UserScores).LoadAsync();
        await context.Reviews.Where(r => r.ProductId == Id).Include(r => r.Score).LoadAsync();
        if (UserScores.Count > 0) AverageUserScore = Math.Round(UserScores.Select(s => s.Value).Average(), 1);
        if (Reviews.Count > 0) AverageReviewScore =
                Math.Round(Reviews.Select(s => s.Score?.Value ?? throw new Exception("Score is null")).Average(), 1);
    }
}
