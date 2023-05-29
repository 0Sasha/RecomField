using System.ComponentModel.DataAnnotations;
namespace RecomField.Models;

public class Book : Product
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Author { get; set; } = default!;
}
