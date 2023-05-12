using iText.Html2pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RecomField.Data;
using RecomField.Hubs;
using RecomField.Models;
namespace RecomField.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;
    private readonly IHubContext<MainHub> hubContext;
    private readonly IStringLocalizer<SharedResource> localizer;

    public ReviewController(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
        IHubContext<MainHub> hubContext, IStringLocalizer<SharedResource> localizer)
    {
        this.userManager = userManager;
        this.context = context;
        this.hubContext = hubContext;
        this.localizer = localizer;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int id)
    {
        var review = await FindReview(id);
        var user = await userManager.GetUserAsync(User);
        await review.LoadAsync(context, user?.Id);
        return View(review);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ConvertToPDF(int id)
    {
        var review = await FindReview(id);
        using MemoryStream pdfDest = new();
        HtmlConverter.ConvertToPdf(await review.ToHtmlAsync(context, localizer), pdfDest);
        return File(new MemoryStream(pdfDest.ToArray()), "application/pdf");
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> GetSimilarReviews(int id)
    {
        var review = await FindReview(id);
        var similar = context.Reviews.Where(r => r.ProductId == review.ProductId && r.Id != id)
            .OrderByDescending(r => r.LikeCounter).Take(10).Include(r => r.Author).Include(r => r.Score);
        ViewData["withProd"] = false;
        return PartialView("ReviewsTableBody", await similar.ToArrayAsync());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ShowComments(int id, int count) =>
        await GetReviewComments(await FindReview(id), count);

    [HttpPost]
    public async Task<IActionResult> AddComment(int id, string comment, int count)
    {
        if (string.IsNullOrEmpty(comment)) throw new ArgumentNullException(nameof(comment));
        var user = await GetUser();
        var review = await FindReview(id);
        review.Comments.Add(new(user, review, comment));
        await context.SaveChangesAsync();
        await hubContext.Clients.All.SendAsync("NewReviewComment", id);
        return await GetReviewComments(review, count);
    }

    private async Task<IActionResult> GetReviewComments(Review review, int count)
    {
        await context.Entry(review).Collection(r => r.Comments).LoadAsync();
        ViewData["id"] = review.Id;
        ViewData["count"] = review.Comments.Count;
        return PartialView("ReviewComments", await context.ReviewComments
            .Where(k => k.Entity == review).Take(count).Include(k => k.Sender).ToArrayAsync());
    }

    public async Task<IActionResult> AddReview(int prodId, string? authorId = null)
    {
        var user = await GetUser();
        authorId ??= user.Id;
        var review = await context.Reviews.SingleOrDefaultAsync(r => r.AuthorId == authorId && r.ProductId == prodId);
        if (review != null) return RedirectToAction(nameof(EditReview), new { id = review.Id });
        var prod = await context.Products.FindAsync(prodId) ?? throw new ArgumentException("Product is not found", nameof(prodId));
        if (authorId == user.Id) return View("EditReview", new Review() { Product = prod, AuthorId = user.Id });
        else if (!User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        var author = await userManager.FindByIdAsync(authorId) ?? throw new Exception("Author is not found");
        return View("EditReview", new Review() { Product = prod, AuthorId = author.Id });
    }

    public async Task<IActionResult> EditReview(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        if (review.Author != user && !User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        await context.Entry(review).Reference(r => r.Product).LoadAsync();
        await context.Entry(review).Reference(r => r.Score).LoadAsync();
        await context.Entry(review).Collection(r => r.Tags).LoadAsync();
        review.Body = review.Body.ReverseCustomizedHtml();
        return View(review);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReview([Bind("Id,AuthorId,ProductId,Title,Body")] Review review)
    {
        var user = await GetUser();
        if (review.AuthorId == null) throw new Exception("AuthorId is null");
        if (review.AuthorId != user.Id && !User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        if (review.ProductId == 0) throw new Exception("ProductId is 0");
        string tags = Request.Form["TagsForServer"].Single() ?? throw new Exception("Tags is not filled");
        int score = int.Parse(Request.Form["RateForServer"].Single() ?? throw new Exception("Score is not defined"));
        return review.Id == 0 ? await AddReview(review, tags, score) :
            await EditReview(review.Id, review.Title, tags, review.Body, score);
    }

    private async Task<IActionResult> AddReview(Review review, string tags, int score)
    {
        review.PublicationDate = DateTime.UtcNow;
        review.Author = await userManager.FindByIdAsync(review.AuthorId) ?? throw new Exception("Author is not found"); ;
        review.Product = await context.Products.FindAsync(review.ProductId) ?? throw new Exception("Product is not found");
        review.Score = new(review.Author, review, score);
        review.Body = review.Body.CustomizeHtmlForView();
        foreach (var tag in tags.Split(",")) review.Tags.Add(new(tag, review));
        await context.AddAsync(review);
        await context.SaveChangesAsync();
        await review.Product.UpdateAverageScoresAsync(context);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id = review.Id });
    }
    
    private async Task<IActionResult> EditReview(int id, string title, string tags, string body, int score)
    {
        var review = await FindReview(id);
        await review.LoadAsync(context, null);
        review.Title = title;
        review.Body = body.CustomizeHtmlForView();
        review.Tags.Clear();
        foreach (var tag in tags.Split(",")) review.Tags.Add(new(tag, review));
        review.Score = new(review.Author, review, score);
        await context.SaveChangesAsync();
        await review.Product.UpdateAverageScoresAsync(context);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id });
    }

    public async Task<IActionResult> RemoveReview(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        if (review.Author != user && !User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        await review.LoadAsync(context, null, true);
        context.Reviews.Remove(review);
        await context.SaveChangesAsync();
        await review.Product.UpdateAverageScoresAsync(context);
        await context.SaveChangesAsync();
        return RedirectToAction("Index", "User", new { id = review.AuthorId });
    }

    [HttpPost]
    public async Task<IActionResult> GetTagList(string partTag)
    {
        var request = "\"" + partTag + "*\" OR \"" + partTag + "\"";
        var tags = await context.ReviewTags
            .Where(t => EF.Functions.Contains(t.Body, request)).Select(t => t.Body).ToArrayAsync();
        return PartialView("OptionsList", tags.Distinct().Take(7));
    }

    [HttpPost]
    public async Task ChangeLike(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        await review.ChangeLikeAsync(context, user);
        await context.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetUser() =>
        await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");

    private async Task<Review> FindReview(int id) =>
        await context.Reviews.FindAsync(id) ?? throw new Exception("Review is not found");
}
