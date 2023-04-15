using Microsoft.AspNetCore.Identity;
namespace RecomField.Models;

public class ApplicationUser : IdentityUser
{
    public bool DarkTheme { get; set; } = true;

    public Language InterfaceLanguage { get; set; } = Language.English;

    public List<Review> Reviews { get; set; } = new();

    public int ReviewLikes { get => Reviews.Select(r => r.Likes.Count).Sum(); }
}
