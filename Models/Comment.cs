using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecomField.Models;

[Index("Id", IsUnique = true)]
public class Comment<T>
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string SenderId { get; set; } = default!;

    [Required]
    [ForeignKey("SenderId")]
    public ApplicationUser Sender { get; set; } = default!;

    public int EntityId { get; set; }

    [Required]
    [ForeignKey("EntityId")]
    public T Entity { get; set; } = default!;

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Body { get; set; } = default!;

    public DateTime PublicationDate { get; set; }

    public Comment() { }

    public Comment(ApplicationUser sender, T entity, string boby)
    {
        Sender = sender;
        Entity = entity;
        Body = boby;
        PublicationDate = DateTime.UtcNow;
    }
}
