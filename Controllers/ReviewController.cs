using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using iText.Html2pdf;
using iText.Layout;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using RecomField.Data;
using RecomField.Hubs;
using RecomField.Models;
using System.Diagnostics.Metrics;
using System.Web;

namespace RecomField.Controllers;

public class ReviewController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;
    private readonly IHubContext<MainHub> hubContext;
    private readonly Cloudinary cloud;

    public ReviewController(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
        Cloudinary cloud, IHubContext<MainHub> hubContext)
    {
        this.userManager = userManager;
        this.context = context;
        this.cloud = cloud;
        this.hubContext = hubContext;
    }

    public async Task<IActionResult> Index(int id)
    {
        var user = await userManager.GetUserAsync(User);
        var review = await FindReview(id);
        await review.LoadAsync(context, user?.Id);
        return View(review);
    }

    [Authorize]
    [HttpGet]
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditReview(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        if (review.Author != user && !User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        await context.Entry(review).Reference(r => r.Product).LoadAsync();
        await context.Entry(review).Reference(r => r.Score).LoadAsync();
        await context.Entry(review).Collection(r => r.Tags).LoadAsync();
        review.Body = ReverseCustomizedStringHtml(review.Body);
        return View(review);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReview([Bind("Id,AuthorId,ProductId,Title,Body")] Review review)
    {
        if (review.AuthorId == null) throw new ArgumentNullException(nameof(review.AuthorId));
        if (review.ProductId == 0) throw new ArgumentNullException(nameof(review.ProductId));
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
        review.Body = CustomizeStringHtml(review.Body);
        foreach (var tag in tags.Split(",")) review.Tags.Add(new(tag, review));
        await context.AddAsync(review);
        await review.Product.UpdateAverageScoresAsync(context);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id = review.Id });
    }
    
    //TODO////////////////////////////////////////////////////////////////////////////////////////
    private async Task<IActionResult> EditReview(int id, string title, string tags, string body, int score)
    {
        var review = await FindReview(id);
        await context.Entry(review).Reference(u => u.Author).LoadAsync();
        await context.Entry(review).Reference(u => u.Product).LoadAsync();
        await context.Entry(review).Reference(u => u.Score).LoadAsync();
        await context.Entry(review).Collection(u => u.Tags).LoadAsync();
        review.Title = title;
        review.Body = CustomizeStringHtml(body); //TODO////////////////////////////////////////////////////////////////
        review.Tags.Clear();
        foreach (var tag in tags.Split(",")) review.Tags.Add(new(tag, review));
        review.Score = new(review.Author, review, score);
        await review.Product.UpdateAverageScoresAsync(context);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id });
    }

    [Authorize]
    public async Task<IActionResult> RemoveReview(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        if (review.Author != user && !User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        await review.LoadAsync(context, user.Id, true);
        context.Reviews.Remove(review);
        await review.Product.UpdateAverageScoresAsync(context);
        await context.SaveChangesAsync();
        return RedirectToAction("Index", "User", new { id = review.AuthorId });
    }

    [HttpPost]
    public async Task<IActionResult> GetTagList(string partTag)
    {
        var t = (await context.ReviewTags.Select(t => t.Body).ToListAsync()).Where(b => b.Contains(partTag, StringComparison.OrdinalIgnoreCase));
        return PartialView("OptionsList", t.Distinct().TakeLast(7));
    }

    [Authorize]
    [HttpPost]
    public async Task UploadImage(IFormFile file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (file.Length > 5000000)
        {
            await Response.WriteAsync("Error: The size of file is more than 5MB");
            return;
        }
        if (!file.ContentType.StartsWith("image"))
        {
            await Response.WriteAsync("Error: The file is not an image");
            return;
        }
        var format = file.ContentType[6..];
        if (format != "jpeg" && format != "jpg" && format != "png")
        {
            await Response.WriteAsync("Error: Incorrect format of image");
            return;
        }
        var uploadParams = new ImageUploadParams() { File = new FileDescription("file", file.OpenReadStream()) };
        var uploadResult = await cloud.UploadAsync(uploadParams);
        await Response.WriteAsync(new { location = uploadResult.Url }.ToJson());
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int id, string comment, int visibleCount)
    {
        if (string.IsNullOrEmpty(comment)) throw new ArgumentNullException(nameof(comment));
        var user = await GetUser();
        var review = await FindReview(id);
        await context.ReviewComments.Where(k => k.Entity == review).Include(k => k.Sender).LoadAsync();
        review.Comments.Add(new(user, review, comment));
        await context.SaveChangesAsync();
        await hubContext.Clients.All.SendAsync("NewReviewComment", id);
        return PartialView("ReviewComments", (review.Comments.Take(visibleCount + 1), review.Id, review.Comments.Count));
    }

    [HttpPost]
    public async Task<IActionResult> ShowComments(int id, int count)
    {
        var review = await FindReview(id);
        await context.ReviewComments.Where(k => k.Entity == review).Include(k => k.Sender).LoadAsync();
        return PartialView("ReviewComments", (review.Comments.Take(count), review.Id, review.Comments.Count));
    }

    [HttpPost]
    public async Task ChangeLike(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        await context.Entry(review).Collection(u => u.Likes).LoadAsync();
        var curLike = review.Likes.SingleOrDefault(l => l.Sender == user);
        if (curLike != null) review.Likes.Remove(curLike);
        else review.Likes.Add(new(user, review));
        review.LikeCounter = review.Likes.Count;
        review.Version = Guid.NewGuid();
        await context.SaveChangesAsync();
    }

    [HttpPost]
    public async Task<IActionResult> GetSimilarReviews(int id)
    {
        var review = await FindReview(id);
        var similar = await context.Reviews.Where(r => r.ProductId == review.ProductId).ToListAsync();
        similar.Remove(review);
        var sorted = similar.OrderByDescending(r => r.LikeCounter).Take(10);
        foreach (var r in sorted)
        {
            await context.Entry(r).Reference(r => r.Author).LoadAsync();
            await context.Entry(r).Reference(r => r.Score).LoadAsync();
        }
        ViewData["withProd"] = false;
        return PartialView("ReviewsTableBody", sorted);
    }

    public async Task<IActionResult> ConvertToPDF(int id)
    {
        var review = await FindReview(id);
        await review.LoadAsync(context, null);
        var backColor = review.Score.Value > 6 ? "forestgreen" : "orange";
        var html = "<h5>Review of " + review.Product.Title + "</h5><style>.badge { background-color: " + backColor + ";color: white; " +
            "padding: 4px 10px; text-align: center; border-radius: 5px;\r\n}</style> <h1>" +
            review.Title + "</h1>" + ClearStringHtmlForPDF(review.Body) + "<h1 style=\"text-align:right;\">" +
            "<span class=\"badge\">" + review.Score.Value + "/10</span></h1>" +
            "<h4 style=\"text-align:right\">By " + review.Author?.UserName + " on " +
            review.PublicationDate.ToString("MMM dd, yyyy") + "</h4>";

        using MemoryStream pdfDest = new();
        HtmlConverter.ConvertToPdf(html, pdfDest);
        return File(new MemoryStream(pdfDest.ToArray()), "application/pdf");
    } //TODO

    private static string ClearStringHtmlForPDF(string body)
    {
        var startImg = body.LastIndexOf("<img");
        while(startImg >= 0)
        {
            body = RemoveAttrsEl(body, startImg, "width", "height", "style");
            var endImg = body.IndexOf(">", startImg);
            body = body.Insert(endImg, " style=\"max-width: 600px; max-height: 840px;\" ");
            startImg = body[..startImg].LastIndexOf("<img");
        }
        var startVideo = body.LastIndexOf("<div class=\"ratio");
        while (startVideo >= 0)
        {
            var endVideo = body.IndexOf("</div>", startVideo);
            body = body.Remove(startVideo, endVideo - startVideo);
            startVideo = body[..startVideo].LastIndexOf("<div class=\"ratio");
        }
        var endGap = body.IndexOf(">&nbsp;</p>");
        while (endGap >= 0)
        {
            var startGap = body[..endGap].LastIndexOf("<");
            body = body.Remove(startGap, endGap + 11 - startGap);
            endGap = body.IndexOf(">&nbsp;</p>");
        }
        return body;
    } //TODO

    private static string RemoveAttrsEl(string body, int startEl, params string[] attrs)
    {
        foreach(var attr in attrs)
        {
            var endEl = body.IndexOf(">", startEl);
            var startAttr = body.IndexOf(attr + "=\"", startEl, endEl - startEl);
            if (startAttr >= 0)
            {
                var endAttr = body.IndexOf("\"", startAttr + attr.Length + 3) + 1;
                body = body.Remove(startAttr, endAttr - startAttr);
            }
        }
        return body;
    }
    
    private static string CustomizeStringHtml(string body) => CustomizeIframe(CustomizeStyle(body));

    private static string CustomizeStyle(string body)
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
        return body;
    } //TODO

    private static string CustomizeIframe(string body)
    {
        var i = body.LastIndexOf("<iframe");
        while (i >= 0)
        {
            var boxEl = body[..i].LastIndexOf("<");
            if (body[boxEl + 1] != 'p')
            {
                //body = SwitchStyle(body, boxEl, i);
                i = body[..i].LastIndexOf("<iframe");
                continue;
            }
            var j = body.IndexOf("</iframe>", i);
            body = body.Insert(j + 9, "</div>");
            var startStyle = body[..j].IndexOf("style=", i);
            if (startStyle >= 0)
            {
                var endStyle = body.IndexOf("\"", startStyle + 7) + 1;
                var style = body[startStyle..endStyle];
                body = body.Remove(startStyle, style.Length);
                body = body.Insert(i, "<div class=\"ratio ratio-16x9\" " + style + ">");
            }
            else body = body.Insert(i, " <div class=\"ratio ratio-16x9\"> ");
            i = body[..i].LastIndexOf("<iframe");
        }
        return body;
    } //TODO

    private static string ReverseCustomizedStringHtml(string body)
    {
        var i = body.LastIndexOf("<iframe");
        while (i >= 0)
        {
            var startBoxEl = body[..i].LastIndexOf("<div");
            var startStyleBox = body.IndexOf("style=", startBoxEl, i - startBoxEl);
            if (startStyleBox >= 0)
            {
                var endStyleBox = body.IndexOf("\"", startStyleBox + 7) + 1;
                var styleBox = body[startStyleBox..endStyleBox];
                body = body.Insert(i + 7, " " + styleBox + " ");
            }
            body = body.Remove(startBoxEl, body.IndexOf(">", startBoxEl) - startBoxEl + 1);
            body = body.Insert(startBoxEl, "<p>");
            var endBoxEl = body.IndexOf("</div>", startBoxEl);
            body = body.Remove(endBoxEl, 6);
            body = body.Insert(endBoxEl, "</p>");
            i = body.LastIndexOf("<iframe", startBoxEl);
        }
        return body;
    } //TODO

    private static string SwitchStyle(string body, int startBox, int startIframe)
    {
        var endIframe = body.IndexOf("</iframe>", startIframe);
        var startStyle = body[..endIframe].IndexOf("style=", startIframe);
        if (startStyle >= 0)
        {
            var endStyle = body.IndexOf("\"", startStyle + 7) + 1;
            var style = body[startStyle..endStyle];
            body = body.Remove(startStyle, style.Length);
            var startBoxStyle = body[..startIframe].IndexOf("style=", startBox);
            if (startBoxStyle >= 0)
            {
                var endBoxStyle = body.IndexOf("\"", startBoxStyle + 7) + 1;
                body = body.Remove(startBoxStyle, endBoxStyle - startBoxStyle);
                body = body.Insert(startBoxStyle, style);
            }
            else body = body.Insert(startBox + 5, " " + style + " ");
        }
        return body;
    } //Remove

    private async Task<ApplicationUser> GetUser() =>
        await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");

    private async Task<Review> FindReview(int id) =>
        await context.Reviews.FindAsync(id) ?? throw new Exception("Review is not found");
}
