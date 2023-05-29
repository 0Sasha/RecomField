using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecomField.Models;

[Index("Id", IsUnique = true)]
public class Tag<T>
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Body { get; set; } = default!;

    public int EntityId { get; set; }

    [Required]
    [ForeignKey("EntityId")]
    public T Entity { get; set; } = default!;

    public Tag() { }

    public Tag(string body, T entity)
    {
        Body = body;
        Entity = entity;
    }

    public override string ToString() => Body ?? "";
}
