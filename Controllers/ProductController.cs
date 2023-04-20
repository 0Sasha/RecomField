using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        await prod.LoadAsync(context);
        return View(prod);
    }

    [HttpGet]
    public IActionResult AddProduct() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct([Bind("Type,Title,ReleaseYear,Description")] Product product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (await context.Product.AnyAsync(p => p.Title == product.Title && p.Type == product.Type && p.ReleaseYear == product.ReleaseYear))
            throw new ArgumentException("The product already exists in the database", nameof(product));
        product.Description ??= "";
        await context.AddAsync(product);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id = product.Id });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> GetProductsForReview(string authorId, string? partTitle = null)
    {
        var user = await GetUser(authorId);
        var reviewed = context.Review.Where(r => r.Author == user).Include(r => r.Product).Select(r => r.Product);
        if (string.IsNullOrEmpty(partTitle))
            return PartialView("ProductsTableBody", await context.Product.Except(reviewed).Take(7).ToArrayAsync());
        var request = "%" + partTitle + "%";
        return PartialView("ProductsTableBody", await
            context.Product.Where(p => EF.Functions.Like(p.Title, request)).Except(reviewed).Take(7).ToArrayAsync());
    }

    [HttpPost]
    public async Task ChangeScoreProduct(int id, int score)
    {
        var user = await GetUser();
        var prod = await FindProduct(id);
        await context.Entry(prod).Collection(p => p.UserScores).LoadAsync();
        var s = prod.UserScores.SingleOrDefault(s => s.Sender == user);
        if (s != null) s.Value = score;
        else prod.UserScores.Add(new(user, prod, score));
        await context.SaveChangesAsync();
    }

    private async Task<Product> FindProduct(int id) =>
        await context.Product.FindAsync(id) ?? throw new Exception("Product is not found");

    private async Task<ApplicationUser> GetUser() =>
        await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");

    private async Task<ApplicationUser> GetUser(string id) =>
        await userManager.FindByIdAsync(id) ?? throw new Exception("User is not found");
}
