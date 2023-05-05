using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Strings;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using static System.Net.Mime.MediaTypeNames;

namespace RecomField.Controllers;

public class ProductController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;

    public ProductController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        this.userManager = userManager;
        this.context = context;
    }

    public async Task<IActionResult> Index(int id)
    {
        var prod = await FindProduct(id);
        var user = await userManager.GetUserAsync(User);
        await prod.LoadAsync(context, user?.Id, true);
        return View(prod);
    }

    [HttpGet]
    public IActionResult AddProduct() => View("AddMovie");

    [HttpGet]
    public IActionResult AddMovie() => View();

    [HttpGet]
    public IActionResult AddSeries() => View();

    [HttpGet]
    public IActionResult AddBook() => View();

    [HttpGet]
    public IActionResult AddGame() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMovie([Bind("Title,ReleaseYear,Description,Cover,Trailer")] Movie product) =>
        await AddProduct(product);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSeries([Bind("Title,ReleaseYear,Description,Cover,Trailer")] Series product) =>
        await AddProduct(product);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGame([Bind("Title,ReleaseYear,Description,Cover,Trailer")] Game product) =>
        await AddProduct(product);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBook([Bind("Title,ReleaseYear,Description,Cover,Author")] Book product) =>
        await AddProduct(product);

    private async Task<IActionResult> AddProduct(Product product)
    {
        var prods = await context.Products.Where(p => p.Title == product.Title &&
        p.ReleaseYear == product.ReleaseYear).ToArrayAsync();
        if (prods.Any(p => p.GetType() == product.GetType()))
            throw new ArgumentException("This product already exists in the database", nameof(product));
        product.Description ??= "";
        if (product is Movie m && m.Trailer != null) m.Trailer = CustomizeLinkToTrailer(m.Trailer);
        else if (product is Series s && s.Trailer != null) s.Trailer = CustomizeLinkToTrailer(s.Trailer);
        else if (product is Game g && g.Trailer != null) g.Trailer = CustomizeLinkToTrailer(g.Trailer);
        await context.AddAsync(product);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id = product.Id });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> GetProductsForReview(string authorId, string? partTitle = null)
    {
        var user = await GetUser(authorId);
        var reviewed = context.Reviews.Where(r => r.Author == user).Include(r => r.Product).Select(r => r.Product);
        ViewData["authorIdForNewReview"] = authorId;
        if (string.IsNullOrEmpty(partTitle))
            return PartialView("ProductsTableBody", await context.Products.Except(reviewed).Take(7).ToListAsync());
        var request = "\"" + partTitle + "*\" OR \"" + partTitle + "\"";
        var byTitle = await context.Products.Where(p => EF.Functions.Contains(p.Title, request)).ToArrayAsync();
        var byAuthor = await context.Books.Where(p => EF.Functions.Contains(p.Author, request)).Select(b => (Product)b).ToArrayAsync();
        var founded = byTitle.Union(byAuthor).Except(reviewed).Take(7).ToList();
        return PartialView("ProductsTableBody", founded);
    }

    [HttpPost]
    public async Task ChangeScoreProduct(int id, int score)
    {
        var user = await GetUser();
        var prod = await FindProduct(id);
        await prod.ChangeUserScoreAsync(context, user, score);
        await context.SaveChangesAsync();
    }

    private async Task<Product> FindProduct(int id) =>
        await context.Products.FindAsync(id) ?? throw new Exception("Product is not found");

    private async Task<ApplicationUser> GetUser() =>
        await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");

    private async Task<ApplicationUser> GetUser(string id) =>
        await userManager.FindByIdAsync(id) ?? throw new Exception("User is not found");

    private static string CustomizeLinkToTrailer(string link)
    {
        var startId = link.IndexOf("v=") + 2;
        var endId = link.IndexOf("&", startId);
        return "https://www.youtube.com/embed/" + (endId >= 0 ? link[startId..endId] : link[startId..]);
    }
}
