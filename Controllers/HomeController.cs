using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using System.Diagnostics;
using NuGet.Protocol;
using Microsoft.AspNetCore.Http;
namespace RecomField.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext context;
    private readonly ILogger<HomeController> logger;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly Cloudinary cloud;

    public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager,
        ApplicationDbContext context, Cloudinary cloud)
    {
        this.logger = logger;
        this.userManager = userManager;
        this.context = context;
        this.cloud = cloud;
    }

    public async Task<IActionResult> Index()
    {
        Response.Cookies.AddUserCookies(await userManager.GetUserAsync(User));
        ViewData["Tags"] = await GetTagsForCloud();
        return View(await context.Movies.OrderByDescending(p => p.AverageUserScore).Take(6).ToArrayAsync());
    }

    private async Task<string> GetTagsForCloud()
    {
        string res = "";
        var tags = await context.ReviewTags.Select(t => t.Body).ToListAsync();
        while (tags.Count > 0)
        {
            var tag = tags[0];
            res += tag + "," + tags.RemoveAll(t => t == tag) + ",";
        }
        return res;
    }

    [HttpPost]
    public async Task<IActionResult> Search(string text, bool products) => products ? await SearchProducts(text) :
        text.StartsWith('[') && text.EndsWith("]") ? await SearchReviewsByTag(text) : await SearchReviews(text);

    private async Task<IActionResult> SearchProducts(string text)
    {
        var request = "\"" + text + "*\" OR \"" + text + "\"";
        var prods = await context.Products.Where(x =>
        EF.Functions.Contains(x.Title, request) || EF.Functions.Contains(x.Description, request)).ToArrayAsync();
        var byAuthor = await context.Books.Where(x => EF.Functions.Contains(x.Author, request)).ToArrayAsync();
        var byYear = int.TryParse(text, out var number) ?
            await context.Products.Where(p => p.ReleaseYear == number).ToArrayAsync() : Array.Empty<Product>();
        return PartialView("ProductsTableBody", prods.Union(byAuthor).Union(byYear));
    }

    private async Task<IActionResult> SearchReviews(string text)
    {
        var request = "\"" + text + "*\" OR \"" + text + "\"";
        var revs = context.Reviews.Where(x => EF.Functions.Contains(x.Title, request) ||
        EF.Functions.Contains(x.Body, request) || EF.Functions.Contains(x.Product.Title, request));
        var byTags = context.ReviewTags.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
        var byComments = context.ReviewComments.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
        var founded = revs.Union(byTags).Union(byComments).Include(r => r.Product).Include(r => r.Author).Include(r => r.Score);
        return PartialView("ReviewsTableBody", await founded.ToArrayAsync());
    }

    private async Task<IActionResult> SearchReviewsByTag(string text)
    {
        text = text[1..(text.Length - 1)];
        var request = "\"" + text + "*\" OR \"" + text + "\"";
        return PartialView("ReviewsTableBody", await context.ReviewTags.Where(x =>
        EF.Functions.Contains(x.Body, request)).Include(r => r.Entity.Product)
        .Include(r => r.Entity.Author).Include(r => r.Entity.Score).Select(t => t.Entity).ToArrayAsync());
    }

    public async Task<IActionResult> ChangeLanguage(string current, string returnUrl)
    {
        if (string.IsNullOrEmpty(current)) throw new ArgumentNullException(nameof(current));
        if (string.IsNullOrEmpty(returnUrl)) throw new ArgumentNullException(nameof(returnUrl));
        var lang = new RequestCulture(current == "en" ? "ru" : "en-US");
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(lang),
            new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) });
        await SaveLanguage(current == "en" ? Language.Russian : Language.English);
        return Redirect(returnUrl);
    }

    private async Task SaveLanguage(Language language)
    {
        var u = await userManager.GetUserAsync(User);
        if (u != null)
        {
            u.InterfaceLanguage = language;
            await userManager.UpdateAsync(u);
        }
    }

    [HttpPost]
    public async Task SaveTheme(bool isDark)
    {
        var u = await userManager.GetUserAsync(User);
        if (u != null)
        {
            u.DarkTheme = isDark;
            await userManager.UpdateAsync(u);
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetBestSeries() => PartialView("BestProductsPartial",
        await context.Series.OrderByDescending(p => p.AverageUserScore).Take(6).ToArrayAsync());

    [HttpPost]
    public async Task<IActionResult> GetBestGames() => PartialView("BestProductsPartial",
        await context.Games.OrderByDescending(p => p.AverageUserScore).Take(6).ToArrayAsync());

    [HttpPost]
    public async Task<IActionResult> GetBestBooks() => PartialView("BestProductsPartial",
        await context.Books.OrderByDescending(p => p.AverageUserScore).Take(6).ToArrayAsync());

    [HttpPost]
    public async Task<IActionResult> GetNewReviews() => PartialView("ReviewsTableBody",
        await context.Reviews.OrderByDescending(r => r.PublicationDate).Take(5)
        .Include(r => r.Product).Include(r => r.Author).Include(r => r.Score).ToArrayAsync());

    [HttpPost]
    public async Task<IActionResult> GetHighScoresReviews() => PartialView("ReviewsTableBody",
        await context.Reviews.OrderByDescending(r => r.Score.Value).Take(5)
        .Include(r => r.Product).Include(r => r.Author).Include(r => r.Score).ToArrayAsync());

    [HttpPost]
    public async Task<IActionResult> GetMostLikedReviews() => PartialView("ReviewsTableBody",
        await context.Reviews.OrderByDescending(r => r.LikeCounter).Take(5)
        .Include(r => r.Product).Include(r => r.Author).Include(r => r.Score).ToArrayAsync());

    [Authorize]
    [HttpPost]
    public async Task UploadImage(IFormFile file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (file.Length > 5000000) await Response.WriteAsync(new { error = "The size of file is more than 5MB" }.ToJson());
        else if (!file.ContentType.StartsWith("image")) await Response.WriteAsync(new { error = "The file is not an image" }.ToJson());
        else if (file.ContentType[6..] != "jpeg" && file.ContentType[6..] != "jpg" && file.ContentType[6..] != "png")
            await Response.WriteAsync(new { error = "Incorrect format of image" }.ToJson());
        else
        {
            var uploadParams = new ImageUploadParams() { File = new FileDescription("file", file.OpenReadStream()) };
            var uploadResult = await cloud.UploadAsync(uploadParams);
            await Response.WriteAsync(new { location = uploadResult.Url }.ToJson());
        }
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task Error()
    {
        Response.StatusCode = 404;
        await Response.CompleteAsync();
    }
}