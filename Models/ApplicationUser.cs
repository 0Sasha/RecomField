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
        if (Reviews.Count > 0) ReviewLikes = Reviews.Select(r => r.Likes.Count).Sum();
    }

    public async Task LoadAllDependent(ApplicationDbContext context)
    {
        await context.Review.Where(r => r.Author == this).Include(r => r.Score).Include(r => r.Tags)
            .Include(r => r.Likes).Include(r => r.Comments).LoadAsync();
        await context.ReviewLike.Where(l => l.Sender == this).LoadAsync();
        await context.ReviewComment.Where(l => l.Sender == this).LoadAsync();
        await context.ReviewScore.Where(l => l.Sender == this).LoadAsync();
    }
}
