using Microsoft.AspNetCore.Identity;
namespace RecomField.Models;

public class ApplicationUser : IdentityUser
{
    public bool DarkTheme { get; set; } = true;
    public Language InterfaceLanguage { get; set; } = Language.English;
}
