using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
namespace RecomField.Models;

public class ApplicationUser : IdentityUser
{
    public bool DarkTheme { get; set; } = true;

    public Language InterfaceLanguage { get; set; } = Language.English;

    public List<Review> Reviews { get; set; } = new();

    public int ReviewLikes { get; set; }
}
