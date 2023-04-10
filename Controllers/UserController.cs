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
        product.Description ??= "";
        await context.AddAsync(product);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(NewReview));
    }

    public IActionResult NewReview() => View("EditReview", new Review());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewReview([Bind("Title,Body,Image,Score")] Review review)
    {
        if (review == null) throw new ArgumentNullException(nameof(review));
        review.Author = await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");
        int idProd = int.Parse(Request.Form["ProductIdForServer"].Single() ?? throw new Exception("Product id is not filled"));
        review.Product = await context.Product.FindAsync(idProd) ?? throw new Exception("Product is not found");
        string tags = Request.Form["TagsForServer"].Single() ?? throw new Exception("Tags is not filled");
        review.Tags = tags.Split(",");
        return RedirectToAction(nameof(MyPage));
    }
}
