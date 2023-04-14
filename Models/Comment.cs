using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecomField.Models;

public class Comment<T>
{
    public int Id { get; set; }

    [Required]
    public string? SenderId { get; set; }

    [Required]
    [ForeignKey("SenderId")]
    public ApplicationUser? Sender { get; set; }

    public int EntityId { get; set; }

    [Required]
    [ForeignKey("EntityId")]
    public T? Entity { get; set; }

    [Required]
    public string? Body { get; set; }

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
