using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace RecomField.Models;

[Index("Id", IsUnique = true)]
public abstract class Product
{
    private int relYear = 2023;

    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public virtual string Title { get; set; } = "";

    public virtual int ReleaseYear
    {
        get => relYear;
        set => relYear = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(ReleaseYear));
    }

    [Required]
    [StringLength(1500, MinimumLength = 1)]
    public virtual string Description { get; set; } = "";

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public virtual string Cover { get; set; } = "";

    public virtual double AverageUserScore { get; set; }

    public virtual double AverageReviewScore { get; set; }

    public virtual List<Score<Product>> UserScores { get; set; } = new();

    public virtual List<Review> Reviews { get; set; } = new();

    public Product() { }

    public override string ToString() => Title;
}
