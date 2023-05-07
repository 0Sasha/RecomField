using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using RecomField.Services;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace RecomField.Controllers;

[Authorize]
public class ProductController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;

    public ProductController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        this.userManager = userManager;
        this.context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int id)
    {
        var prod = await FindProduct(id);
        var user = await userManager.GetUserAsync(User);
        await prod.LoadAsync(context, user?.Id, true);
        return View(prod);
    }

    public IActionResult AddProduct(string type)
    {
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));
        if (type == "Movie") return View(new Movie());
        if (type == "Series") return View(new Series());
        if (type == "Book") return View(new Book());
        if (type == "Game") return View(new Game());
        throw new ArgumentException("Unexpected value", nameof(type));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMovie([Bind("Title,ReleaseYear,Description,Cover,Trailer")] Movie product)
    {
        product.Trailer?.CustomizeYouTubeLink();
        return await AddProduct(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSeries([Bind("Title,ReleaseYear,Description,Cover,Trailer")] Series product)
    {
        product.Trailer?.CustomizeYouTubeLink();
        return await AddProduct(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGame([Bind("Title,ReleaseYear,Description,Cover,Trailer")] Game product)
    {
        product.Trailer?.CustomizeYouTubeLink();
        return await AddProduct(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBook([Bind("Title,ReleaseYear,Description,Cover,Author")] Book product) =>
        await AddProduct(product);

    private async Task<IActionResult> AddProduct(Product product)
    {
        var prods = await context.Products
            .Where(p => p.Title == product.Title && p.ReleaseYear == product.ReleaseYear).ToArrayAsync();
        if (prods.Any(p => p.GetType() == product.GetType()))
            throw new ArgumentException("This product already exists in the database", nameof(product));
        await context.AddAsync(product);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id = product.Id });
    }

    [HttpPost]
    public async Task<IActionResult> GetProductsForReview(string authorId, string? partTitle = null)
    {
        var user = await userManager.FindByIdAsync(authorId) ?? throw new Exception("User is not found");
        var reviewed = context.Reviews.Where(r => r.Author == user).Include(r => r.Product).Select(r => r.Product);
        ViewData["authorIdForNewReview"] = authorId;
        if (string.IsNullOrEmpty(partTitle))
            return PartialView("ProductsTableBody", await context.Products.Except(reviewed).Take(7).ToArrayAsync());
        var request = "\"" + partTitle + "*\" OR \"" + partTitle + "\"";
        var byTitle = await context.Products.Where(p => EF.Functions.Contains(p.Title, request)).ToArrayAsync();
        var byAuthor = await context.Books.Where(p => EF.Functions.Contains(p.Author, request)).Select(b => (Product)b).ToArrayAsync();
        return PartialView("ProductsTableBody", byTitle.Union(byAuthor).Except(reviewed).Take(7));
    }

    [HttpPost]
    public async Task ChangeScoreProduct(int id, int score)
    {
        var user = await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");
        var prod = await FindProduct(id);
        await prod.ChangeUserScoreAsync(context, user, score);
        await context.SaveChangesAsync();
    }

    private async Task<Product> FindProduct(int id) =>
        await context.Products.FindAsync(id) ?? throw new Exception("Product is not found");
}
