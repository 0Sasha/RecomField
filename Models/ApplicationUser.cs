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
        if (deep) await context.Reviews.Where(r => r.Author == this)
                .Include(r => r.Product).Include(r => r.Score).LoadAsync();
        else await context.Entry(this).Collection(r => r.Reviews).LoadAsync();
        if (Reviews.Count > 0) ReviewLikes = Reviews.Select(r => r.LikeCounter).Sum();
    }

    public async Task LoadAllDependent(ApplicationDbContext context)
    {
        await context.Reviews.Where(r => r.Author == this).Include(r => r.Score).Include(r => r.Tags)
            .Include(r => r.Likes).Include(r => r.Comments).LoadAsync();
        await context.ReviewLikes.Where(l => l.Sender == this).LoadAsync();
        await context.ReviewComments.Where(l => l.Sender == this).LoadAsync();
        await context.ReviewScores.Where(l => l.Sender == this).LoadAsync();
        await context.ProductScores.Where(l => l.Sender == this).LoadAsync();
    }
}
