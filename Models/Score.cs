using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecomField.Models;

public class Score<T>
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

    [Range(0, int.MaxValue)]
    public int Value { get; set; }

    public Score() { }

    public Score(ApplicationUser sender, T entity, int value)
    {
        Sender = sender;
        Entity = entity;
        Value = value;
    }
}
