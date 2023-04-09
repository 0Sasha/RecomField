namespace RecomField.Models;

public class Product
{
    private int relYear = 2023;

    public int Id { get; set; }

    public virtual ProductType Type { get; set; }

    public virtual string Title { get; set; }

    public virtual int ReleaseYear
    {
        get => relYear;
        set => relYear = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(ReleaseYear));
    }

    public virtual string Description { get; set; } = "";

    public virtual double UserScore { get; set; }

    public virtual double ReviewerScore { get; set; }

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
