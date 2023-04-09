using Microsoft.AspNetCore.Identity;
namespace RecomField.Models;

public class Review
{
    public IdentityUser Author { get; set; }

    public Product Product { get; set; }

    public string Title { get; set; }

    public string[] Tags { get; set; }

    public string Body { get; set; }

    public string? Image { get; set; }

    public int Score { get; set; }

    public Review(IdentityUser author) => Author = author ?? throw new ArgumentNullException(nameof(author));

    public Review(IdentityUser author, Product product, string title, string[] tags, string body, string? image, int score)
    {
        Author = author;
        Product = product;
        Title = title;
        Tags = tags;
        Body = body;
        Image = image;
        Score = score;
    }
}
