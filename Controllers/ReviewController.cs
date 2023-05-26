using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Hubs;
using RecomField.Models;
using RecomField.Services;
using System.Security.Claims;
namespace RecomField.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly ApplicationDbContext context;
    private readonly IHubContext<MainHub> hubContext;
    private readonly IReviewService<Review> reviewService;
    private readonly IProductService<Product> productService;

    public ReviewController(ApplicationDbContext context, IHubContext<MainHub> hubContext,
        IReviewService<Review> reviewService, IProductService<Product> productService)
    {
        this.context = context;
        this.hubContext = hubContext;
        this.reviewService = reviewService;
        this.productService = productService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int id) =>
        View(await reviewService.LoadReviewAsync(id, false, User.FindFirstValue(ClaimTypes.NameIdentifier)));

    [AllowAnonymous]
    public async Task<IActionResult> ConvertToPDF(int id) =>
        File(new MemoryStream(await reviewService.GetPdfVersionAsync(id)), "application/pdf");

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> GetSimilarReviews(int id)
    {
        ViewData["withProd"] = false;
        return PartialView("ReviewsTableBody", await reviewService.GetSimilarReviewsAsync(id, 10));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> ShowComments(int id, int count) => await GetReviewComments(id, count);

    [HttpPost]
    public async Task<IActionResult> AddComment(int id, string comment, int count)
    {
        await reviewService.AddCommentAsync(id, GetUserId(), comment);
        await hubContext.Clients.Group("review" + id).SendAsync("NewReviewComment", id);
        return await GetReviewComments(id, count);
    }

    private async Task<IActionResult> GetReviewComments(int id, int count)
    {
        var review = await context.Reviews.FindAsync(id) ?? throw new Exception("Review is not found");
        await context.Entry(review).Collection(r => r.Comments).LoadAsync();
        ViewData["id"] = review.Id;
        ViewData["count"] = review.Comments.Count;
        return PartialView("ReviewComments", await context.ReviewComments
            .Where(k => k.Entity == review).Take(count).Include(k => k.Sender).ToArrayAsync());
    }

    public async Task<IActionResult> AddReview(int prodId, string? authorId = null)
    {
        var userId = GetUserId();
        authorId ??= userId;
        var review = await context.Reviews.SingleOrDefaultAsync(r => r.AuthorId == authorId && r.ProductId == prodId);
        if (review != null) return RedirectToAction(nameof(EditReview), new { id = review.Id });

        CheckAccess(authorId);
        var prod = await context.Products.FindAsync(prodId) ??
            throw new ArgumentException("Product is not found", nameof(prodId));
        var author = await context.Users.FindAsync(authorId) ?? throw new Exception("Author is not found");
        return View("EditReview", new Review() { Product = prod, AuthorId = authorId });
    }

    public async Task<IActionResult> EditReview(int id)
    {
        var review = await reviewService.LoadReviewAsync(id, false, null);
        CheckAccess(review.AuthorId);
        review.Body = review.Body.ReverseCustomizedHtml();
        return View(review);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReview([Bind("Id,AuthorId,ProductId,Title,Body")] Review review)
    {
        CheckAccess(review.AuthorId);
        if (ModelState.Any(s => s.Key != "Score" && s.Key != "Author" &&
        s.Key != "Product" && s.Value?.ValidationState != ModelValidationState.Valid))
        {
            review.Product = await context.Products.FindAsync(review.ProductId) ?? throw new Exception();
            return View(review);
        }
        var tags = Request.Form["TagsForServer"].Single()?.Split(",") ?? throw new Exception("Tags is not filled");
        var score = int.Parse(Request.Form["RateForServer"].Single() ?? throw new Exception("Score is not defined"));
        return await EditReview(review, score, tags);
    }

    private async Task<IActionResult> EditReview(Review review, int score, string[] tags)
    {
        if (review.Id == 0) await reviewService.AddReviewAsync(review, score, tags);
        else await reviewService.EditReviewAsync(review.Id, review.Title, review.Body, score, tags);
        await productService.UpdateAverageScoresAsync(review.ProductId);
        return RedirectToAction(nameof(Index), new { id = review.Id });
    }

    public async Task<IActionResult> RemoveReview(int id)
    {
        var review = await reviewService.LoadReviewAsync(id, true, null);
        CheckAccess(review.AuthorId);
        context.Reviews.Remove(review);
        await context.SaveChangesAsync();
        await productService.UpdateAverageScoresAsync(review.ProductId);
        return RedirectToAction("Index", "User", new { id = review.AuthorId });
    }

    [HttpPost]
    public async Task<IActionResult> GetTagList(string partTag) =>
        PartialView("OptionsList", await reviewService.GetReviewTagsAsync(partTag, 7));

    [HttpPost]
    public async Task ChangeLike(int id) => await reviewService.ChangeLikeAsync(id, GetUserId());

    private void CheckAccess(string authorId)
    {
        if (authorId != GetUserId() && !User.IsInRole("Admin"))
            throw new Exception("User is not an author or admin");
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId is not found");
}
