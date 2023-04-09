using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;

namespace RecomField.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;

    public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        this.userManager = userManager;
        this.context = context;
    }

    public IActionResult MyPage()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetProductsView(string partTitle) // TODO/////////////////////////////////////////
    {
        var prods = await context.Product.ToArrayAsync();
        if (!string.IsNullOrEmpty(partTitle))
            prods = prods.Where(p => p.Title.Contains(partTitle, StringComparison.OrdinalIgnoreCase)).ToArray();
        return PartialView("ProductsTableBody", prods.TakeLast(7).Reverse());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct([Bind("Type,Title,ReleaseYear,Description")] Product product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (await context.Product.AnyAsync(p => p.Title == product.Title && p.Type == product.Type && p.ReleaseYear == product.ReleaseYear))
            throw new ArgumentException("The product already exists in the database", nameof(product));
        if (product.Description == null) product.Description = "";
        await context.AddAsync(product);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(NewReview));
    }

    public async Task<IActionResult> NewReview() =>
        View("EditReview", new Review(await userManager.GetUserAsync(User) ?? throw new Exception("User in not found")));
}
