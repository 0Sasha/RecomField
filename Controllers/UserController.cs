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
        var user = (id == null ? await userManager.GetUserAsync(User) :
            await userManager.FindByIdAsync(id)) ?? throw new Exception("User is not found");
        await user.LoadAsync(context, true);
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> SearchUsers(string? text = null) // Very slow search - add fulltext///////////////
    {
        var roles = await context.UserRoles.ToListAsync();
        var users = await (string.IsNullOrEmpty(text) ? context.Users : context.Users
            .Where(u => u.UserName.Contains(text))).Take(10).ToListAsync();
        return PartialView("UsersTableBody", (users, roles));
    }

    [HttpPost]
    public async Task<IActionResult> GetReviewsView(string? id = null, string? search = null)
    {
        var user = await userManager.GetUserAsync(User);
        var u = id == null ? user : await userManager.FindByIdAsync(id) ?? throw new Exception("User is not found");
        await u.LoadAsync(context, true);
        return PartialView("ReviewsTableBody", search == null ? (u.Reviews, user == u) :
            (u.Reviews.Where(r => r.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            r.Product.Title.Contains(search, StringComparison.OrdinalIgnoreCase)), user == u));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminPage()
    {
        var roles = await context.UserRoles.ToListAsync();
        var users = await context.Users.Take(10).ToListAsync();
        return View((users, roles));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> BlockUser(string id, int? days = null, string? filter = null)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        user.LockoutEnd = days == null ? DateTimeOffset.MaxValue : DateTimeOffset.UtcNow.AddDays((double)days);
        await context.SaveChangesAsync();
        if (user == await userManager.GetUserAsync(User)) await signInManager.SignOutAsync();
        return await SearchUsers(filter);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> UnlockUser(string id, string? filter = null)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        user.LockoutEnd = DateTimeOffset.MinValue;
        await context.SaveChangesAsync();
        return await SearchUsers(filter);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RemoveUser(string id, string? filter = null)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        await user.LoadAllDependent(context);
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return await SearchUsers(filter);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> AddAdminRole(string id, string? filter = null)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        if (await userManager.IsInRoleAsync(user, "Admin")) throw new Exception("User is already an admin");
        await userManager.AddToRoleAsync(user, "Admin");
        await context.SaveChangesAsync();
        return await SearchUsers(filter);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RevokeAdminRole(string id, string? filter = null)
    {
        var user = await context.Users.FindAsync(id) ?? throw new Exception("User is not found");
        if (!await userManager.IsInRoleAsync(user, "Admin")) throw new Exception("User is not an admin");
        await userManager.RemoveFromRoleAsync(user, "Admin");
        await context.SaveChangesAsync();
        return await SearchUsers(filter);
    }
}
