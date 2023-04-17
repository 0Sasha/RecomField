using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecomField.Models;

public class Tag<T>
{
    public int Id { get; set; }

    [Required]
    public string? Body { get; set; }

    public int EntityId { get; set; }

    [Required]
    [ForeignKey("EntityId")]
    public T? Entity { get; set; }

    public Tag() { }

    public Tag(string body, T entity)
    {
        Body = body;
        Entity = entity;
    }
}
