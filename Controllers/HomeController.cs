﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using RecomField.Services;
using System.Security.Claims;
namespace RecomField.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext context;
    private readonly ILogger<HomeController> logger;
    private readonly IUserService<ApplicationUser, IResponseCookies, Language> userService;
    private readonly IProductService<Product> productService;
    private readonly IReviewService<Review> reviewService;
    private readonly ICloudService<IFormFile> cloudService;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,
        IUserService<ApplicationUser, IResponseCookies, Language> userService, IProductService<Product> productService,
        ICloudService<IFormFile> cloudService, IReviewService<Review> reviewService)
    {
        this.logger = logger;
        this.context = context;
        this.userService = userService;
        this.cloudService = cloudService;
        this.productService = productService;
        this.reviewService = reviewService;
    }

    public async Task<IActionResult> Index()
    {
        await userService.AddUserCookiesAsync(GetUserId(), Response.Cookies);
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
        await SearchReviews(text, text.StartsWith('[') && text.EndsWith("]"));

    private async Task<IActionResult> SearchProducts(string text) =>
        PartialView("ProductsTableBody", await productService.GetProductsAsync(30, text, Array.Empty<Product>()));

    private async Task<IActionResult> SearchReviews(string text, bool byTag) =>
        PartialView("ReviewsTableBody", await reviewService.GetReviewsAsync(text, byTag));

    public async Task<IActionResult> ChangeLanguage(string current, string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl)) throw new ArgumentNullException(nameof(returnUrl));
        var lang = new RequestCulture(current == "en" ? "ru" : "en-US");
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(lang),
            new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) });
        await userService.SaveLanguageAsync(GetUserId(), current == "en" ? Language.Russian : Language.English);
        return Redirect(returnUrl);
    }

    [HttpPost]
    public async Task SaveTheme(bool isDark) => await userService.SaveThemeAsync(GetUserId(), isDark);

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
    public async Task UploadImage(IFormFile file) =>
        await Response.WriteAsync(await cloudService.UploadImageAsync(file));

    public IActionResult Privacy() => View();

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task Error()
    {
        Response.StatusCode = 404;
        await Response.CompleteAsync();
    }
}