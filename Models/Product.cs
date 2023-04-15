using System.ComponentModel.DataAnnotations;
namespace RecomField.Models;

public class Product
{
    private int relYear = 2023;

    public int Id { get; set; }

    public virtual ProductType Type { get; set; }

    [Required]
    public virtual string? Title { get; set; }

    public virtual int ReleaseYear
    {
        get => relYear;
        set => relYear = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(ReleaseYear));
    }

    public virtual string Description { get; set; } = "";

    public virtual string? Cover { get; set; }

    public virtual string? Trailer { get; set; }

    public virtual double AverageUserScore { get; set; }

    public virtual double AverageReviewScore { get; set; }

    public virtual List<Score<Product>> UserScores { get; set; } = new();

    public virtual List<Review> Reviews { get; set; } = new();

    public Product() { }

    public Product(ProductType type, string title, int releaseYear)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
        Type = type;
        Title = title;
        ReleaseYear = releaseYear;
    }

    public enum ProductType
    {
        Movie,
        Series,
        Game,
        Book
    }
}
