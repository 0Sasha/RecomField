using Microsoft.AspNetCore.Identity;
namespace RecomField.Models;

public class Review
{
    public int Id { get; set; }

    public ApplicationUser Author { get; set; }

    public Product Product { get; set; }

    public string Title { get; set; }

    public string[] Tags { get; set; }

    public string Body { get; set; }

    public string? Image { get; set; }

    public int Score { get; set; }

    public Review() { }
}
