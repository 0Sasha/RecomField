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
public class ProductController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;
    private readonly IProductService<Product> productService;

    public ProductController(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
        IProductService<Product> productService)
    {
        this.userManager = userManager;
        this.context = context;
        this.productService = productService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int id) =>
        View(await productService.LoadProductAsync(id, true, GetUserId()));

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
        product.Trailer = product.Trailer?.CustomizeYouTubeLink();
        return await AddProduct(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSeries([Bind("Title,ReleaseYear,Description,Cover,Trailer")] Series product)
    {
        product.Trailer = product.Trailer?.CustomizeYouTubeLink();
        return await AddProduct(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGame([Bind("Title,ReleaseYear,Description,Cover,Trailer")] Game product)
    {
        product.Trailer = product.Trailer?.CustomizeYouTubeLink();
        return await AddProduct(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBook([Bind("Title,ReleaseYear,Description,Cover,Author")] Book product) =>
        await AddProduct(product);

    private async Task<IActionResult> AddProduct(Product product)
    {
        if (!ModelState.IsValid) return View("AddProduct", product);
        if (!await productService.AddProductAsync(product))
        {
            ModelState.AddModelError("", "This product already exists in the database");
            return View("AddProduct", product);
        }
        return RedirectToAction(nameof(Index), new { id = product.Id });
    }

    [HttpPost]
    public async Task<IActionResult> GetProductsForReview(string authorId, string? partTitle = null)
    {
        var reviewed =
            context.Reviews.Where(r => r.AuthorId == authorId).Include(r => r.Product).Select(r => r.Product);
        ViewData["authorIdForNewReview"] = authorId;
        return PartialView("ProductsTableBody", await productService.GetProductsAsync(7, partTitle, reviewed));
    }

    [HttpPost]
    public async Task ChangeScoreProduct(int id, int score) =>
        await productService.ChangeUserScoreAsync(id, GetUserId(), score);

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId is not found");
}
