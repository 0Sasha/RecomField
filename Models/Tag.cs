using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RecomField.Models;

public class Tag
{
    public int Id { get; set; }

    [Required]
    public string? Body { get; set; }

    public Tag() { }

    public Tag(string body) => Body = body;
}
