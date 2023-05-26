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
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;
    private readonly IUserService<ApplicationUser, IResponseCookies, Language> userService;

    public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
        IUserService<ApplicationUser, IResponseCookies, Language> userService)
    {
        this.userManager = userManager;
        this.context = context;
        this.userService = userService;
    }

    public async Task<IActionResult> Index(string? id = null) =>
        View(await userService.LoadUserAsync(string.IsNullOrEmpty(id) ? GetUserId() : id));

    [HttpPost]
    public async Task<IActionResult> GetReviewsView(string userId, string? search, string sort, bool ascOrder)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(sort)) throw new ArgumentNullException(nameof(sort));
        var curUser = await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");
        var user = await userService.LoadUserAsync(userId);
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
    public async Task<IActionResult> GetUsersView(string? search, string type, int count)
    {
        ViewData["Admins"] = await context.UserRoles.Select(r => r.UserId).ToArrayAsync();
        return PartialView("UsersTableBody", await userService.GetUsersAsync(type, count, search));
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
