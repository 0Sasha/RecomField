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
        var r = await context.Review.FindAsync(id);
        await context.Entry(r).Reference(u => u.Author).LoadAsync();
        await context.Entry(r).Reference(u => u.Product).LoadAsync();
        await context.Entry(r).Reference(u => u.Score).LoadAsync();
        await context.Entry(r).Collection(u => u.Tags).LoadAsync();
        await context.Entry(r).Collection(u => u.Likes).LoadAsync();
        await context.Entry(r).Collection(u => u.Comments).LoadAsync();
        return View(r);
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
        return RedirectToAction(nameof(AddReview));
    }

    [HttpGet]
    public async Task<IActionResult> EditReview(int id = 0)
    {
        if (id == 0) return View(new Review());
        else
        {
            var r = await context.Review.FindAsync(id);
            var user = await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");
            if (user != r.Author) throw new Exception("User is not author of review"); ////////////////TODO ADMIN
            await context.Entry(r).Reference(u => u.Score).LoadAsync();
            await context.Entry(r).Collection(u => u.Tags).LoadAsync();
            return View(r);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReview([Bind("Title,Body")] Review review)
    {
        if (review.Title == null) throw new ArgumentNullException(nameof(review.Title));
        if (review.Body == null) throw new ArgumentNullException(nameof(review.Body));
        var user = await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");
        string tags = Request.Form["TagsForServer"].Single() ?? throw new Exception("Tags is not filled");
        int score = int.Parse(Request.Form["RateForServer"].Single() ?? throw new Exception("Score is not defined"));
        string? id = Request.Form["IdForServer"].SingleOrDefault();
        return id == null ? await AddReview(user, review, tags, score) :
            await EditReview(user, int.Parse(id), review.Title, tags, review.Body, score);
    }

    private async Task<IActionResult> AddReview(ApplicationUser user, Review review, string tags, int score)
    {
        review.PublicationDate = DateTime.UtcNow;
        review.Author = user;
        int idProd = int.Parse(Request.Form["ProductIdForServer"].Single() ?? throw new Exception("Product id is not filled"));
        review.Product = await context.Product.FindAsync(idProd) ?? throw new Exception("Product is not found");
        review.Score = new(user, review, score);
        review.Body = CustomizeStringHtml(review.Body);
        foreach (var tag in tags.Split(",")) review.Tags.Add(new(tag));
        await context.AddAsync(review);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Review), new { id = review.Id });
    }

    private async Task<IActionResult> EditReview(ApplicationUser user, int id, string title, string tags, string body, int score)
    {
        var review = await context.Review.FindAsync(id);
        await context.Product.FindAsync(review.ProductId);
        await context.Entry(review).Reference(u => u.Score).LoadAsync();
        await context.Entry(review).Collection(u => u.Tags).LoadAsync();
        review.Title = title;
        review.Body = CustomizeStringHtml(body); //TODO//////////////////////////
        review.Tags = new();
        foreach (var tag in tags.Split(",")) review.Tags.Add(new(tag));
        review.Score = new(user, review, score);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Review), new { id });
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

    [HttpPost]
    public async Task ChangeLike(int id)
    {
        var user = await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");
        var r = await context.Review.FindAsync(id) ?? throw new Exception("Review is not found");
        await context.Entry(r).Collection(u => u.Likes).LoadAsync();
        var like = r.Likes.SingleOrDefault(l => l.Sender == user);
        if (like != null) r.Likes.Remove(like);
        else r.Likes.Add(new(user, r));
        await context.SaveChangesAsync();
    }
}
