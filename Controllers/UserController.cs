using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using RecomField.Data;
using RecomField.Models;
using System.IO;

namespace RecomField.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;
    private readonly IWebHostEnvironment appEnvironment;
    private readonly Cloudinary cloud;

    public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment appEnvironment,
        Cloudinary cloud)
    {
        this.userManager = userManager;
        this.context = context;
        this.appEnvironment = appEnvironment;
        this.cloud = cloud;
    }

    public async Task<IActionResult> MyPage()
    {
        var u = await userManager.GetUserAsync(User);
        return View(context.Review.Where(r => r.Author == u).Include(r => r.Product).AsEnumerable());
    }

    public async Task<IActionResult> Review(int id)
    {
        return View(await context.Review.Include(r => r.Author).Include(r => r.Product).SingleAsync(r => r.Id == id));
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
    public async Task<IActionResult> NewReview([Bind("Title,Body")] Review review)
    {
        if (review == null) throw new ArgumentNullException(nameof(review));
        review.PublicationDate = DateTime.UtcNow;
        review.Author = await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");
        int idProd = int.Parse(Request.Form["ProductIdForServer"].Single() ?? throw new Exception("Product id is not filled"));
        review.Product = await context.Product.FindAsync(idProd) ?? throw new Exception("Product is not found");
        review.Score = int.Parse(Request.Form["RateForServer"].Single() ?? throw new Exception("Score is not defined"));
        string tags = Request.Form["TagsForServer"].Single() ?? throw new Exception("Tags is not filled");
        review.Tags = tags.Split(",");
        review.Body = CustomizeStringHtml(review.Body);
        await context.AddAsync(review);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(MyPage));
    }

    private static string CustomizeStringHtml(string body) // TODO/////////////////////////////////////////
    {
        if (body.Contains("style=\""))
        {
            var b = body.Split("\"");
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i].EndsWith("style=") && i + 1 < b.Length)
                {
                    if (b[i + 1].StartsWith("height") || b[i + 1].StartsWith("width")) b[i + 1] = b[i + 1].Insert(0, "max-");
                    b[i + 1] = b[i + 1].Replace(" width", " max-width");
                    b[i + 1] = b[i + 1].Replace(" height", " max-height");
                }
            }
            body = string.Join("\"", b);
        }
        if (body.Contains("<iframe"))
        {
            var i = body.LastIndexOf("<iframe");
            while (i >= 0)
            {
                var j = body.IndexOf("</iframe>", i);
                var startStyle = body[..j].IndexOf("style=", i);
                if (startStyle >= 0)
                {
                    var endStyle = body.IndexOf("\"", startStyle + 7) + 1;
                    var style = body[startStyle..endStyle];
                    body = body.Remove(startStyle, style.Length);
                    body = body.Insert(i, "<div class=\"ratio ratio-16x9\" " + style + ">");
                }
                i = body[..i].LastIndexOf("<iframe");
            }
            body = body.Replace("</iframe>", "</iframe></div>");
        }
        return body;
    }

    [HttpPost]
    public async Task UploadImage(IFormFile file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (file.Length > 5000000)
        {
            await Response.WriteAsync("The size of file is more than 5MB");
            return;
        }
        var uploadParams = new ImageUploadParams() { File = new FileDescription("file", file.OpenReadStream()) };
        var uploadResult = await cloud.UploadAsync(uploadParams);
        await Response.WriteAsync(new { location = uploadResult.Url }.ToJson());
    }
}
