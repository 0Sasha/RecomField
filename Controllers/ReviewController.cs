using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using iText.Html2pdf;
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
        var review = await FindReview(id);
        await review.LoadAsync(context);
        var user = await userManager.GetUserAsync(User);
        if (user != null)
            await context.ProductScore.SingleOrDefaultAsync(s => s.Entity == review.Product && s.Sender == user);
        return View(review);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> AddReview(string? authorId = null)
    {
        var user = await GetUser();
        if (authorId == null) return View("EditReview", new Review() { AuthorId = user.Id });
        else if (authorId != user.Id && !User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        else
        {
            var author = await userManager.FindByIdAsync(authorId) ?? throw new Exception("Author is not found");
            return View("EditReview", new Review() { AuthorId = author.Id });
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditReview(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        if (review.Author != user && !User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        await context.Entry(review).Reference(r => r.Score).LoadAsync();
        await context.Entry(review).Collection(r => r.Tags).LoadAsync();
        return View(review);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReview([Bind("AuthorId,Title,Body")] Review review)
    {
        if (review.Title == null) throw new ArgumentNullException(nameof(review.Title));
        if (review.Body == null) throw new ArgumentNullException(nameof(review.Body));
        if (review.AuthorId == null) throw new ArgumentNullException(nameof(review.Body));
        var author = await userManager.FindByIdAsync(review.AuthorId) ?? throw new Exception("Author is not found");
        string tags = Request.Form["TagsForServer"].Single() ?? throw new Exception("Tags is not filled");
        int score = int.Parse(Request.Form["RateForServer"].Single() ?? throw new Exception("Score is not defined"));
        string? id = Request.Form["IdForServer"].SingleOrDefault();
        return id == null ? await AddReview(author, review, tags, score) :
            await EditReview(author, int.Parse(id), review.Title, tags, review.Body, score);
    }

    private async Task<IActionResult> AddReview(ApplicationUser author, Review review, string tags, int score)
    {
        review.PublicationDate = DateTime.UtcNow;
        review.Author = author;
        int idProd = int.Parse(Request.Form["ProductIdForServer"].Single() ?? throw new Exception("Product id is not filled"));
        review.Product = await context.Product.FindAsync(idProd) ?? throw new Exception("Product is not found");
        review.Score = new(author, review, score);
        review.Body = CustomizeStringHtml(review.Body);
        foreach (var tag in tags.Split(",")) review.Tags.Add(new(tag, review));
        await context.AddAsync(review);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id = review.Id });
    }
    
    //TODO////////////////////////////////////////////////////////////////////////////////////////
    private async Task<IActionResult> EditReview(ApplicationUser author, int id, string title, string tags, string body, int score)
    {
        var review = await FindReview(id);
        await context.Product.FindAsync(review.ProductId);
        await context.Entry(review).Reference(u => u.Score).LoadAsync();
        await context.Entry(review).Collection(u => u.Tags).LoadAsync();
        review.Title = title;
        review.Body = CustomizeStringHtml(body); //TODO////////////////////////////////////////////////////////////////
        review.Tags.Clear();
        foreach (var tag in tags.Split(",")) review.Tags.Add(new(tag, review));
        review.Score = new(author, review, score);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { id });
    }

    [Authorize]
    public async Task<IActionResult> RemoveReview(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        if (review.Author != user && !User.IsInRole("Admin")) throw new Exception("User is not an author or admin");
        await review.LoadAsync(context);
        context.Review.Remove(review);
        await context.SaveChangesAsync();
        return RedirectToAction("Index", "User", new { id = review.AuthorId });
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

    [Authorize]
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
    public async Task<IActionResult> AddComment(int id, string comment, int visibleCount)
    {
        if (string.IsNullOrEmpty(comment)) throw new ArgumentNullException(nameof(comment));
        var user = await GetUser();
        var review = await FindReview(id);
        await context.ReviewComment.Where(k => k.Entity == review).Include(k => k.Sender).LoadAsync();
        review.Comments.Add(new(user, review, comment));
        await context.SaveChangesAsync();
        await hubContext.Clients.All.SendAsync("NewReviewComment", id);
        return PartialView("ReviewComments", (review.Comments.Take(visibleCount + 1), review.Id, review.Comments.Count));
    }

    [HttpPost]
    public async Task<IActionResult> ShowComments(int id, int count)
    {
        var review = await FindReview(id);
        await context.ReviewComment.Where(k => k.Entity == review).Include(k => k.Sender).LoadAsync();
        return PartialView("ReviewComments", (review.Comments.Take(count), review.Id, review.Comments.Count));
    }

    [HttpPost]
    public async Task ChangeLike(int id)
    {
        var user = await GetUser();
        var review = await FindReview(id);
        await context.Entry(review).Collection(u => u.Likes).LoadAsync();
        await context.Entry(review).Reference(u => u.Author).LoadAsync();
        var like = review.Likes.SingleOrDefault(l => l.Sender == user);
        if (like != null) review.Likes.Remove(like);
        else review.Likes.Add(new(user, review));
        await context.SaveChangesAsync();
    }

    public async Task<IActionResult> DownloadReview(int id)
    {
        var review = await FindReview(id);
        await review.LoadAsync(context);

        var html = "<h1>" + review.Title + "</h1>\r\n<h2>By " + review.Author?.UserName + " on " +
            review.PublicationDate.ToString("MMM dd, yyyy") + "</h2>\r\n\r\n" + review.Body;

        using MemoryStream pdfDest = new();
        //ConverterProperties converterProperties = new();
        HtmlConverter.ConvertToPdf(html, pdfDest);
        return File(new MemoryStream(pdfDest.ToArray()), "application/pdf");
    }

    public virtual string RenderViewToString(
    ControllerContext controllerContext,
    string viewPath,
    string masterPath,
    ViewDataDictionary viewData,
    TempDataDictionary tempData)
    {
        Stream filter = null;
        ViewResult v = new();
        //ViewPage viewPage = new ViewPage();
        ViewContext viewContext = new ViewContext();
        //Right, create our view
        //var v = new ViewContext(

        //Get the response context, flush it and get the response filter.
        /*var response = viewPage.ViewContext.HttpContext.Response;
        response.Flush();
        var oldFilter = response.Filter;

        try
        {
            //Put a new filter into the response
            filter = new MemoryStream();
            response.Filter = filter;

            //Now render the view into the memorystream and flush the response
            viewPage.ViewContext.View.Render(viewPage.ViewContext, viewPage.ViewContext.HttpContext.Response.Output);
            response.Flush();

            //Now read the rendered view.
            filter.Position = 0;
            var reader = new StreamReader(filter, response.ContentEncoding);
            return reader.ReadToEnd();
        }
        finally
        {
            //Clean up.
            if (filter != null)
            {
                filter.Dispose();
            }

            //Now replace the response filter
            response.Filter = oldFilter;
        }*/
        return "";
    }

    private async Task<ApplicationUser> GetUser() =>
        await userManager.GetUserAsync(User) ?? throw new Exception("User is not found");

    private async Task<Review> FindReview(int id) =>
        await context.Review.FindAsync(id) ?? throw new Exception("Review is not found");
}
