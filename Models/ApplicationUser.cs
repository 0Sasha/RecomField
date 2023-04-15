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

    public async Task LoadAsync(ApplicationDbContext context, bool deep = false)
    {
        if (deep) await context.Review.Where(r => r.Author == this)
                .Include(r => r.Product).Include(r => r.Score).Include(r => r.Likes).LoadAsync();
        else await context.Review.Where(r => r.Author == this).Include(r => r.Likes).LoadAsync();
        ReviewLikes = Reviews.Select(r => r.Likes.Count).Sum();
    }
}
