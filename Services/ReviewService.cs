using iText.Html2pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RecomField.Data;
using RecomField.Models;
namespace RecomField.Services;

public class ReviewService : IReviewService<Review>
{
    private readonly ApplicationDbContext context;
    private readonly IStringLocalizer<SharedResource> localizer;

    public ReviewService(ApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        this.context = context;
        this.localizer = localizer;
    }

    public async Task<Review[]> GetReviewsAsync(string search, bool byTag = false)
    {
        if (byTag) search = search[1..(search.Length - 1)];
        var request = "\"" + search + "*\" OR \"" + search + "\"";
        return byTag ? await SearchByTagAsync(request) : await SearchAsync(request);
    }

    private async Task<Review[]> SearchAsync(string request)
    {
        var revs = context.Reviews.Where(x => EF.Functions.Contains(x.Title, request) ||
        EF.Functions.Contains(x.Body, request) || EF.Functions.Contains(x.Product.Title, request));
        var byTags = context.ReviewTags.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
        var byComments = context.ReviewComments.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
        var united = revs.Union(byTags).Union(byComments);
        return await united.Include(r => r.Product).Include(r => r.Author).Include(r => r.Score).ToArrayAsync();
    }

    private async Task<Review[]> SearchByTagAsync(string request) =>
        await context.ReviewTags.Where(x => EF.Functions.Contains(x.Body, request)).Include(r => r.Entity.Product)
        .Include(r => r.Entity.Author).Include(r => r.Entity.Score).Select(t => t.Entity).ToArrayAsync();

    public async Task<Review> LoadReviewAsync(int reviewId, bool deep, string? userId)
    {
        var review = await GetReviewAsync(reviewId);
        if (review.Author == null) await context.Entry(review).Reference(r => r.Author).LoadAsync();
        if (review.Product == null) await context.Entry(review).Reference(r => r.Product).LoadAsync();
        if (review.Score == null) await context.Entry(review).Reference(u => u.Score).LoadAsync();
        if (review.Tags.Count == 0) await context.Entry(review).Collection(u => u.Tags).LoadAsync();
        if (deep)
        {
            await context.Entry(review).Collection(u => u.Likes).LoadAsync();
            await context.Entry(review).Collection(u => u.Comments).LoadAsync();
        }
        if (userId != null)
        {
            await context.ReviewLikes.SingleOrDefaultAsync(l => l.SenderId == userId && l.EntityId == reviewId);
            await context.ProductScores.SingleOrDefaultAsync(s => s.SenderId == userId && s.EntityId == review.ProductId);
        }
        return review;
    }

    public async Task<byte[]> GetPdfVersionAsync(int reviewId)
    {
        var review = await LoadReviewAsync(reviewId, false, null);
        using MemoryStream pdfDest = new();
        HtmlConverter.ConvertToPdf(ConstructHtml(review), pdfDest);
        return pdfDest.ToArray();
    }

    private string ConstructHtml(Review review)
    {
        var backColor = review.Score.Value > 6 ? "forestgreen" : "orange";
        return "<h5>" + localizer["Review of"] + " " + review.Product.Title +
            "</h5><style>.badge { background-color: " + backColor + ";color: white; " +
            "padding: 4px 10px; text-align: center; border-radius: 5px;\r\n}</style> <h1>" +
            review.Title + "</h1>" + review.Body.CustomizeHtmlForPDF() + "<h1 style=\"text-align:right;\">" +
            "<span class=\"badge\">" + review.Score.Value + "/10</span></h1>" +
            "<h4 style=\"text-align:right\">" + localizer["By"] + " " + review.Author.UserName + " " +
            localizer["on"] + " " + review.PublicationDate.ToString("dd MMM yyyy") + "</h4>";
    }

    public async Task ChangeLikeAsync(int reviewId, string userId)
    {
        var review = await GetReviewAsync(reviewId);
        await context.Entry(review).Collection(u => u.Likes).LoadAsync();
        var curLike = review.Likes.SingleOrDefault(l => l.SenderId == userId);
        if (curLike != null) review.Likes.Remove(curLike);
        else review.Likes.Add(
            new(await context.Users.FindAsync(userId) ?? throw new Exception("User is not found"), review));
        review.LikeCounter = review.Likes.Count;
        review.Version = Guid.NewGuid();
        await context.SaveChangesAsync();
    }

    private async Task<Review> GetReviewAsync(int reviewId) =>
        await context.Reviews.FindAsync(reviewId) ?? throw new Exception("Review is not found");
}
