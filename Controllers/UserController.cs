using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
namespace RecomField.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly ApplicationDbContext context;

    public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
        ApplicationDbContext context)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.context = context;
    }

    public async Task<IActionResult> Index(string? id = null)
    {
        var user = (string.IsNullOrEmpty(id) ? await userManager.GetUserAsync(User) :
            await userManager.FindByIdAsync(id)) ?? throw new Exception("User is not found");
        await user.LoadAsync(context, true);
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> GetUsersView(string? search, string type, int count)
    {
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (count < 1) count = int.MaxValue;
        var admins = await context.UserRoles.Select(r => r.UserId).ToArrayAsync();
        ViewData["Admins"] = admins;
        return PartialView("UsersTableBody", await FilterUsers(search, type, count, admins));
    }

    private async Task<ApplicationUser[]> FilterUsers(string? search, string type, int count, string[] admins)
    {
        var users = string.IsNullOrEmpty(search) ? context.Users :
            context.Users.Where(u => u.UserName != null && u.UserName.Contains(search));
        return type == "Admins" ? await users.Where(u => admins.Contains(u.Id)).Take(count).ToArrayAsync() :
            type == "All" ? await users.Take(count).ToArrayAsync() :
            type == "Blocked" ? await users.Where(u => u.LockoutEnd != null &&
            u.LockoutEnd != DateTimeOffset.MinValue).Take(count).ToArrayAsync() :
            throw new ArgumentException("Unexpected value", nameof(type));
    }

    [HttpPost]
    public async Task<IActionResult> GetReviewsView(string userId, string? search, string sort, bool ascOrder)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(sort)) throw new ArgumentNullException(nameof(sort));
        var curUser = await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");
        var user = await userManager.FindByIdAsync(userId) ?? throw new Exception("User is not found");
        await user.LoadAsync(context, true);
        ViewData["addMenu"] = user == curUser || await userManager.IsInRoleAsync(curUser, "Admin");
        return PartialView("ReviewsTableBody", await FilterReviews(user, search, sort, ascOrder));
    }

    private async Task<Review[]> FilterReviews(ApplicationUser user, string? search, string sort, bool ascOrder)
    {
        Review[] filtered;
        if (!string.IsNullOrEmpty(search))
        {
            var request = "\"" + search + "*\" OR \"" + search + "\"";
            filtered = await context.Reviews.Where(r => r.AuthorId == user.Id &&
            (EF.Functions.Contains(r.Title, request) || EF.Functions.Contains(r.Body, request) ||
            EF.Functions.Contains(r.Product.Title, request))).ToArrayAsync();
        }
        else filtered = user.Reviews.ToArray();
        return SortReviews(filtered, sort, ascOrder);
    }

    private static Review[] SortReviews(IEnumerable<Review> reviews, string sort, bool ascOrder)
    {
        if (sort == "date") return ascOrder ? reviews.OrderBy(r => r.PublicationDate).ToArray() :
                reviews.OrderByDescending(r => r.PublicationDate).ToArray();
        if (sort == "title") return ascOrder ? reviews.OrderBy(r => r.Title).ToArray() :
                reviews.OrderByDescending(r => r.Title).ToArray();
        if (sort == "likes") return ascOrder ? reviews.OrderBy(r => r.LikeCounter).ToArray() :
                reviews.OrderByDescending(r => r.LikeCounter).ToArray();
        if (sort == "score") return ascOrder ? reviews.OrderBy(r => r.Score.Value).ToArray() :
                reviews.OrderByDescending(r => r.Score.Value).ToArray();
        throw new ArgumentException("Unexpected value", nameof(sort));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminPage()
    {
        var users = await context.Users.Take(10).ToArrayAsync();
        ViewData["Admins"] = await context.UserRoles.Select(r => r.UserId).ToArrayAsync();
        ViewData["CountUsers"] = await context.Users.CountAsync();
        return View(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task BlockUser(string id, int? days = null)
    {
        var user = await GetUserAsync(id);
        user.LockoutEnd = days == null ? DateTimeOffset.MaxValue : DateTimeOffset.UtcNow.AddDays(days.Value);
        await userManager.UpdateSecurityStampAsync(user);
        await context.SaveChangesAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task UnlockUser(string id)
    {
        var user = await GetUserAsync(id);
        user.LockoutEnd = DateTimeOffset.MinValue;
        await context.SaveChangesAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task RemoveUser(string id)
    {
        var user = await GetUserAsync(id);
        await user.LoadAllDependent(context);
        var revs = await context.ReviewLikes.Where(l => l.Sender == user).Select(l => l.Entity).ToListAsync();
        foreach (var r in revs) r.LikeCounter--;
        // Can update averageScores
        context.Users.Remove(user);
        if (user == await userManager.GetUserAsync(User)) await signInManager.SignOutAsync();
        await context.SaveChangesAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task AddAdminRole(string id)
    {
        var user = await GetUserAsync(id);
        if (await userManager.IsInRoleAsync(user, "Admin")) throw new Exception("User is already an admin");
        await userManager.AddToRoleAsync(user, "Admin");
        await context.SaveChangesAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task RevokeAdminRole(string id)
    {
        var user = await GetUserAsync(id);
        if (!await userManager.IsInRoleAsync(user, "Admin")) throw new Exception("User is not an admin");
        await userManager.RemoveFromRoleAsync(user, "Admin");
        await userManager.UpdateSecurityStampAsync(user);
        await context.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetUserAsync(string id) =>
        await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
}
