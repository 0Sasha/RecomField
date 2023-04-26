using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using System.ComponentModel.DataAnnotations;
using static System.Formats.Asn1.AsnWriter;

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

    [Required]
    [MinLength(10)]
    public virtual string Description { get; set; } = "";

    public virtual string? Cover { get; set; }

    public virtual double AverageUserScore { get; set; }

    public virtual double AverageReviewScore { get; set; }

    public virtual List<Score<Product>> UserScores { get; set; } = new();

    public virtual List<Review> Reviews { get; set; } = new();

    public Product() { }

    public virtual async Task LoadAsync(ApplicationDbContext context, string? userId, bool deep = false)
    {
        if (userId != null) await context.ProductScores.SingleOrDefaultAsync(s => s.SenderId == userId && s.EntityId == Id);
        if (deep) await context.Reviews.Where(r => r.ProductId == Id).Include(r => r.Score).Include(r => r.Author).LoadAsync();
    }

    public virtual async Task ChangeUserScoreAsync(ApplicationDbContext context, ApplicationUser user, int score)
    {
        await context.Entry(this).Collection(p => p.UserScores).LoadAsync();
        var s = UserScores.SingleOrDefault(s => s.Sender == user);
        if (s != null) s.Value = score;
        else UserScores.Add(new(user, this, score));
        AverageUserScore = Math.Round(UserScores.Select(s => s.Value).Average(), 2);
    }

    public virtual async Task UpdateAvScoresAsync(ApplicationDbContext context)
    {
        await context.Entry(this).Collection(p => p.UserScores).LoadAsync();
        await context.Reviews.Where(r => r.ProductId == Id).Include(r => r.Score).LoadAsync();
        AverageUserScore = UserScores.Count > 0 ? Math.Round(UserScores.Select(s => s.Value).Average(), 2) : 0;
        AverageReviewScore = Reviews.Count > 0 ?
            Math.Round(Reviews.Select(s => s.Score?.Value ?? throw new Exception("Score is null")).Average(), 2) : 0;
    }
}
