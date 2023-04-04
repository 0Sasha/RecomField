using Microsoft.AspNetCore.Identity;
namespace RecomField.Models;

public class ApplicationUser : IdentityUser
{
    public UserSettings Settings { get; set; } = new();
}
