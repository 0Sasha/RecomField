using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using RecomField.Services;
using System.Diagnostics;
using System.Linq;
namespace RecomField.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext context;
    private readonly ILogger<HomeController> logger;
    private readonly UserManager<ApplicationUser> userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        this.logger = logger;
        this.userManager = userManager;
        this.context = context;
    }

    public async Task<IActionResult> Index()
    {
        /*var prods = await context.Products.ToArrayAsync();
        foreach (var p in prods) await p.UpdateAvScoresAsync(context);

        var revs = await context.Reviews.ToArrayAsync();
        foreach (var r in revs) await r.UpdateLikeCounter(context);
        await context.SaveChangesAsync();*/

        Response.Cookies.AddUserCookies(await userManager.GetUserAsync(User));
        var movies = await context.Movies.OrderByDescending(p => p.AverageUserScore).Take(6).ToArrayAsync();
        return View((movies, await GetTagsForCloud()));
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

    public async Task<IActionResult> Search(string text, bool products) => products ?
        await SearchProducts(text) : text.StartsWith('[') && text.EndsWith("]") ?
            await SearchReviewsByTag(text[1..(text.Length - 1)]) : await SearchReviews(text);

    private async Task<IActionResult> SearchProducts(string text)
    {
        var request = "\"" + text + "*\" OR \"" + text + "\"";
        var byTitle = await context.Products.Where(x => EF.Functions.Contains(x.Title, request)).ToArrayAsync();
        var byDescription = await context.Products.Where(x => EF.Functions.Contains(x.Description, request)).ToArrayAsync();
        var byAuthor = (await context.Books.Where(x => EF.Functions.Contains(x.Author, request)).ToArrayAsync()).Select(b => (Product)b);
        return PartialView("ProductsTableBody", byTitle.Union(byDescription).Union(byAuthor));
    }

    private async Task<IActionResult> SearchReviews(string text)
    {
        var request = "\"" + text + "*\" OR \"" + text + "\"";
        var byTitle = context.Reviews.Where(x => EF.Functions.Contains(x.Title, request));
        var byBody = context.Reviews.Where(x => EF.Functions.Contains(x.Body, request));
        var byProduct = context.Reviews.Where(x => EF.Functions.Contains(x.Product.Title, request));
        var byTags = context.ReviewTags.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
        var byComments = context.ReviewComments.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
        var founded = byTitle.Union(byBody).Union(byProduct).Union(byTags).Union(byComments);
        return PartialView("ReviewsTableBody", await founded.Include(r => r.Product).Include(r => r.Author).Include(r => r.Score).ToArrayAsync());
    }

    private async Task<IActionResult> SearchReviewsByTag(string text)
    {
        var request = "\"" + text + "*\" OR \"" + text + "\"";
        var res = context.ReviewTags.Where(x => EF.Functions.Contains(x.Body, request)).Include(r => r.Entity.Product).Include(r => r.Entity.Author).Include(r => r.Entity.Score);
        return PartialView("ReviewsTableBody", await res.Select(t => t.Entity).ToArrayAsync());
    }

    public async Task<IActionResult> ChangeLanguage(string current, string returnUrl)
    {
        if (string.IsNullOrEmpty(current)) throw new ArgumentNullException(nameof(current));
        if (string.IsNullOrEmpty(returnUrl)) throw new ArgumentNullException(nameof(returnUrl));
        var lang = new RequestCulture(current == "en" ? "ru" : "en-US");
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(lang),
            new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) });
        var u = await userManager.GetUserAsync(User);
        if (u != null)
        {
            u.InterfaceLanguage = current == "en" ? Language.Russian : Language.English;
            await userManager.UpdateAsync(u);
        }
        return Redirect(returnUrl);
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

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}