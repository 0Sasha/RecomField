using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecomField.Models;

public class Review
{
    public int Id { get; set; }

    [Required]
    public string? AuthorId { get; set; }

    [Required]
    [ForeignKey("AuthorId")]
    public ApplicationUser? Author { get; set; }

    public int ProductId { get; set; }

    [Required]
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [Required]
    public string? Title { get; set; }

    [Required]
    [MinLength(10)]
    public string? Body { get; set; }

    [Required]
    public Score<Review>? Score { get; set; }

    [DataType(DataType.Date)]
    public DateTime PublicationDate { get; set; }

    public List<Tag> Tags { get; set; } = new();

    public List<Like<Review>> Likes { get; set; } = new();

    public List<Comment<Review>> Comments { get; set; } = new();

    public void AddLike(ApplicationUser sender)
    {
        Likes.Add(new(sender, this));
    }

    public void RemoveLike(ApplicationUser sender)
    {
        Likes.Remove(Likes.Single(l => l.Sender == sender));
    }
}
