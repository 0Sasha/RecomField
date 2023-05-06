using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecomField.Models;

public class Like<T>
{
    public int Id { get; set; }

    [Required]
    [MinLength(1)]
    public string SenderId { get; set; }

    [Required]
    [ForeignKey("SenderId")]
    public ApplicationUser Sender { get; set; }

    public int EntityId { get; set; }

    [Required]
    [ForeignKey("EntityId")]
    public T Entity { get; set; }

    public Like() { }

    public Like(ApplicationUser sender, T entity)
    {
        Sender = sender;
        Entity = entity;
    }
}
