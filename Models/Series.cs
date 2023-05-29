using System.ComponentModel.DataAnnotations;
namespace RecomField.Models;

public class Series : Product
{
    [StringLength(500, MinimumLength = 10)]
    public string? Trailer { get; set; }
}
