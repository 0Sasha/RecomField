using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace RecomField.Models;

[Index("Id", IsUnique = true)]
public class Review
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string AuthorId { get; set; } = default!;

    [Required]
    [ForeignKey("AuthorId")]
    public ApplicationUser Author { get; set; } = default!;

    public int ProductId { get; set; }

    [Required]
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = default!;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = default!;

    [Required]
    [StringLength(50000, MinimumLength = 1)]
    public string Body { get; set; } = default!;

    [Required]
    public Score<Review> Score { get; set; } = default!;

    [DataType(DataType.Date)]
    public DateTime PublicationDate { get; set; }

    public List<Tag<Review>> Tags { get; set; } = new();

    public List<Like<Review>> Likes { get; set; } = new();

    public List<Comment<Review>> Comments { get; set; } = new();

    public int LikeCounter { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; }
}
