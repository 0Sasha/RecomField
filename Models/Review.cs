using Microsoft.AspNetCore.Identity;
using RecomField.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace RecomField.Models;

[Index("Id", IsUnique = true)]
public class Review
{
    public int Id { get; set; }

    [Required]
    [MinLength(1)]
    public string AuthorId { get; set; }

    [Required]
    [ForeignKey("AuthorId")]
    public ApplicationUser Author { get; set; }

    public int ProductId { get; set; }

    [Required]
    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [Required]
    [MinLength(1)]
    public string Title { get; set; }

    [Required]
    [MinLength(1)]
    public string Body { get; set; }

    [Required]
    public Score<Review> Score { get; set; }

    [DataType(DataType.Date)]
    public DateTime PublicationDate { get; set; }

    public List<Tag<Review>> Tags { get; set; } = new();

    public List<Like<Review>> Likes { get; set; } = new();

    public List<Comment<Review>> Comments { get; set; } = new();

    public int LikeCounter { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; }

    public Review() { }

    public async Task LoadAsync(ApplicationDbContext context, string? userId, bool deep = false)
    {
        if (Author == null) await context.Entry(this).Reference(r => r.Author).LoadAsync();
        if (Product == null)
        {
            await context.Entry(this).Reference(r => r.Product).LoadAsync();
            if (Product == null) throw new Exception("Product is not found");
        }
        await Product.LoadAsync(context, userId);
        if (Score == null) await context.Entry(this).Reference(u => u.Score).LoadAsync();
        if (Tags.Count == 0) await context.Entry(this).Collection(u => u.Tags).LoadAsync();
        if (deep)
        {
            await context.Entry(this).Collection(u => u.Likes).LoadAsync();
            await context.Entry(this).Collection(u => u.Comments).LoadAsync();
        }
        else if (userId != null)
            await context.ReviewLikes.SingleOrDefaultAsync(l => l.SenderId == userId && l.EntityId == Id);
    }

    public async Task ChangeLikeAsync(ApplicationDbContext context, ApplicationUser user)
    {
        await context.Entry(this).Collection(u => u.Likes).LoadAsync();
        var curLike = Likes.SingleOrDefault(l => l.Sender == user);
        if (curLike != null) Likes.Remove(curLike);
        else Likes.Add(new(user, this));
        LikeCounter = Likes.Count;
        Version = Guid.NewGuid();
    }

    public async Task<string> ToHtmlAsync(ApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        if (Author == null) await context.Entry(this).Reference(r => r.Author).LoadAsync();
        if (Product == null) await context.Entry(this).Reference(r => r.Product).LoadAsync();
        if (Score == null) await context.Entry(this).Reference(u => u.Score).LoadAsync();
        return ConstructHtml(localizer);
    }

    private string ConstructHtml(IStringLocalizer<SharedResource> localizer)
    {
        var backColor = Score.Value > 6 ? "forestgreen" : "orange";
        return "<h5>" + localizer["Review of"] + " " + Product.Title +
            "</h5><style>.badge { background-color: " + backColor + ";color: white; " +
            "padding: 4px 10px; text-align: center; border-radius: 5px;\r\n}</style> <h1>" +
            Title + "</h1>" + Body.CustomizeHtmlForPDF() + "<h1 style=\"text-align:right;\">" +
            "<span class=\"badge\">" + Score.Value + "/10</span></h1>" +
            "<h4 style=\"text-align:right\">" + localizer["By"] + " " + Author.UserName + " " +
            localizer["on"] + " " + PublicationDate.ToString("dd MMM yyyy") + "</h4>";
    }
}
