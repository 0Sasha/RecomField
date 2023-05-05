using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using System.Security.Claims;

namespace RecomField.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly ApplicationDbContext context;

    public UserController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, 
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
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
    public async Task<IActionResult> GetUsersView(string? search, string type, int count) // Very slow search - add fulltext///////////////
    {
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (count <= 0) count = int.MaxValue;
        var roles = await context.UserRoles.ToListAsync();
        var users = string.IsNullOrEmpty(search) ? context.Users : context.Users.Where(u => u.UserName.Contains(search));
        List<ApplicationUser> selected;
        if (type == "All") selected = await users.Take(count).ToListAsync();
        else if (type == "Admins")
        {
            var idAdmins = roles.Select(r => r.UserId).ToList();
            selected = await users.Where(u => idAdmins.Contains(u.Id)).Take(count).ToListAsync();
        }
        else if (type == "Blocked") selected = await users.Where(u => u.LockoutEnd != null && u.LockoutEnd != DateTimeOffset.MinValue).Take(count).ToListAsync();
        else throw new ArgumentException("Unexpected value", nameof(type));
        return PartialView("UsersTableBody", (selected, roles));
    }

    [HttpPost]
    public async Task<IActionResult> GetReviewsView(string userId, string? search, string sort, bool ascOrder)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(sort)) throw new ArgumentNullException(nameof(sort));
        var curUser = await userManager.GetUserAsync(User);
        var user = await userManager.FindByIdAsync(userId) ?? throw new Exception("User is not found");
        await user.LoadAsync(context, true);
        List<Review> founded = user.Reviews;
        if (!string.IsNullOrEmpty(search))
        {
            var request = "\"" + search + "*\" OR \"" + search + "\"";
            founded = await context.Reviews.Where(r => r.AuthorId == userId &&
            (EF.Functions.Contains(r.Title, request) || EF.Functions.Contains(r.Body, request) ||
            EF.Functions.Contains(r.Product.Title, request))).ToListAsync();
        }
        ViewData["addMenu"] = user == curUser || await userManager.IsInRoleAsync(user, "Admin");
        return PartialView("ReviewsTableBody", SortReviews(founded, sort, ascOrder));
    }

    private static IEnumerable<Review> SortReviews(IEnumerable<Review> reviews, string sort, bool ascOrder)
    {
        if (!reviews.Any()) return reviews;
        if (sort == "date") return ascOrder ? reviews.OrderBy(r => r.PublicationDate).ToList() :
                reviews.OrderByDescending(r => r.PublicationDate).ToList();
        if (sort == "title") return ascOrder ? reviews.OrderBy(r => r.Title).ToList() :
                reviews.OrderByDescending(r => r.Title).ToList();
        if (sort == "likes") return ascOrder ? reviews.OrderBy(r => r.LikeCounter).ToList() :
                reviews.OrderByDescending(r => r.LikeCounter).ToList();
        if (sort == "score") return ascOrder ? reviews.OrderBy(r => r.Score.Value).ToList() :
                reviews.OrderByDescending(r => r.Score.Value).ToList();
        throw new ArgumentException("Unexpected value", nameof(sort));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminPage()
    {
        var roles = await context.UserRoles.ToListAsync();
        var users = await context.Users.Take(10).ToListAsync();
        return View((users, roles, await context.Users.CountAsync()));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task BlockUser(string id, int? days = null)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        user.LockoutEnd = days == null ? DateTimeOffset.MaxValue : DateTimeOffset.UtcNow.AddDays((double)days);
        await context.SaveChangesAsync();
        await userManager.UpdateSecurityStampAsync(user);
        if (user == await userManager.GetUserAsync(User)) await signInManager.SignOutAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task UnlockUser(string id)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        user.LockoutEnd = DateTimeOffset.MinValue;
        await context.SaveChangesAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task RemoveUser(string id)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        await user.LoadAllDependent(context);
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        if (user == await userManager.GetUserAsync(User)) await signInManager.SignOutAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task AddAdminRole(string id)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        if (await userManager.IsInRoleAsync(user, "Admin")) throw new Exception("User is already an admin");
        await userManager.AddToRoleAsync(user, "Admin");
        await context.SaveChangesAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task RevokeAdminRole(string id)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        if (!await userManager.IsInRoleAsync(user, "Admin")) throw new Exception("User is not an admin");
        await userManager.RemoveFromRoleAsync(user, "Admin");
        await context.SaveChangesAsync();
    }
}
