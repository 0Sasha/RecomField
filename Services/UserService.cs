using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
namespace RecomField.Services;

public class UserService : IUserService<ApplicationUser, IResponseCookies>
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<ApplicationUser> userManager;

    public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    public async Task<ApplicationUser> LoadUserAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        await context.Reviews.Where(r => r.Author == user).Include(r => r.Product).Include(r => r.Score).LoadAsync();
        if (user.Reviews.Count > 0) user.ReviewLikes = user.Reviews.Select(r => r.LikeCounter).Sum();
        else user.ReviewLikes = 0;
        return user;
    }

    public async Task<ApplicationUser[]> GetUsersAsync(int count, string? search, string? type)
    {
        if (count < 1) count = int.MaxValue;
        var users = string.IsNullOrEmpty(search) ? context.Users :
            context.Users.Where(u => u.UserName != null && u.UserName.Contains(search));
        if (type == null || type == "All") return await users.Take(count).ToArrayAsync();
        if (type == "Admins")
        {
            var adminsId = await context.UserRoles.Select(r => r.UserId).ToArrayAsync();
            return await users.Where(u => adminsId.Contains(u.Id)).Take(count).ToArrayAsync();
        }
        if (type == "Blocked")
            return await users.Where(u => u.LockoutEnd > DateTimeOffset.UtcNow).Take(count).ToArrayAsync();
        throw new ArgumentException("Unexpected value", nameof(type));
    }

    public async Task AddUserCookiesAsync(string userId, IResponseCookies cookies)
    {
        var user = await GetUserAsync(userId);
        var opt = new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) };
        var lang = new RequestCulture(user.InterfaceLanguage == Language.Russian ? "ru" : "en-US");
        cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(lang), opt);
        cookies.Append("IsDarkTheme", user.DarkTheme.ToString(), opt);
    }

    public async Task SaveLanguageAsync(string userId, string language)
    {
        var user = await GetUserAsync(userId);
        if (language == "en-US") user.InterfaceLanguage = Language.English;
        else if (language == "ru") user.InterfaceLanguage = Language.Russian;
        else throw new ArgumentException("Unexpected value", nameof(language));
        await userManager.UpdateAsync(user);
    }

    public async Task SaveThemeAsync(string userId, bool isDark)
    {
        var user = await GetUserAsync(userId);
        user.DarkTheme = isDark;
        await userManager.UpdateAsync(user);
    }

    public async Task AddAdminRoleAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        if (await userManager.IsInRoleAsync(user, "Admin")) throw new Exception("User is already an admin");
        await userManager.AddToRoleAsync(user, "Admin");
        await context.SaveChangesAsync();
    }

    public async Task RevokeAdminRoleAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        if (!await userManager.IsInRoleAsync(user, "Admin")) throw new Exception("User is not an admin");
        await userManager.RemoveFromRoleAsync(user, "Admin");
        await userManager.UpdateSecurityStampAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task BlockUserAsync(string userId, int? days)
    {
        var user = await GetUserAsync(userId);
        user.LockoutEnd = days == null ? DateTimeOffset.MaxValue : DateTimeOffset.UtcNow.AddDays(days.Value);
        await userManager.UpdateSecurityStampAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UnblockUserAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        user.LockoutEnd = DateTimeOffset.MinValue;
        await context.SaveChangesAsync();
    }

    public async Task RemoveUserAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        await context.Reviews.Where(r => r.Author == user).Include(r => r.Score).Include(r => r.Tags)
            .Include(r => r.Likes).Include(r => r.Comments).LoadAsync();
        await context.ReviewLikes.Where(l => l.Sender == user).LoadAsync();
        await context.ReviewComments.Where(l => l.Sender == user).LoadAsync();
        await context.ReviewScores.Where(l => l.Sender == user).LoadAsync();
        await context.ProductScores.Where(l => l.Sender == user).LoadAsync();
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetUserAsync(string userId) =>
        await context.Users.FindAsync(userId) ?? throw new Exception("User is not found");
}
