using System.ComponentModel.DataAnnotations;
namespace RecomField.Models;

public class Book : Product
{
    [Required]
    [MinLength(1)]
    public string Author { get; set; } = "";
}
