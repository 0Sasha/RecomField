using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace RecomField.Models;

public class Review
{
    public int Id { get; set; }

    [Required]
    public ApplicationUser Author { get; set; }

    [Required]
    public Product Product { get; set; }

    [Required]
    public string Title { get; set; }

    public string[] Tags { get; set; }

    [Required]
    [MinLength(10)]
    public string Body { get; set; }

    [Required]
    public int Score { get; set; }

    [DataType(DataType.Date)]
    public DateTime PublicationDate { get; set; }

    public Review() { }
}
