using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using RecomField.Services;
using System.Security.Claims;
namespace RecomField.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext context;
    private readonly IUserService<ApplicationUser, IResponseCookies> userService;
    private readonly IReviewService<Review> reviewService;

    public UserController(ApplicationDbContext context,
        IUserService<ApplicationUser, IResponseCookies> userService, IReviewService<Review> reviewService)
    {
        this.context = context;
        this.userService = userService;
        this.reviewService = reviewService;
    }

    public async Task<IActionResult> Index(string? id = null) =>
        View(await userService.LoadUserAsync(string.IsNullOrEmpty(id) ? GetUserId() : id));

    [HttpPost]
    public async Task<IActionResult> GetReviewsView(string userId, string? search, string sort, bool ascOrder)
    {
        ViewData["addMenu"] = userId == GetUserId() || User.IsInRole("Admin");
        return PartialView("ReviewsTableBody",
            reviewService.SortReviews(await reviewService.GetReviewsAsync(userId, search), sort, ascOrder));
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
    public async Task<IActionResult> GetUsersView(string? search, string type, int count)
    {
        ViewData["Admins"] = await context.UserRoles.Select(r => r.UserId).ToArrayAsync();
        return PartialView("UsersTableBody", await userService.GetUsersAsync(count, search, type));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task BlockUser(string id, int? days) => await userService.BlockUserAsync(id, days);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task UnlockUser(string id) => await userService.UnblockUserAsync(id);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task RemoveUser(string id)
    {
        var revs = await context.ReviewLikes.Where(l => l.SenderId == id).Select(l => l.Entity).ToArrayAsync();
        await userService.RemoveUserAsync(id);
        foreach (var r in revs) r.LikeCounter--;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task AddAdminRole(string id) => await userService.AddAdminRoleAsync(id);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task RevokeAdminRole(string id) => await userService.RevokeAdminRoleAsync(id);

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId is not found");
}
