using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Migrations;
using RecomField.Models;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using static iText.Svg.SvgConstants;
using static System.Formats.Asn1.AsnWriter;

namespace RecomField.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<HomeController> logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserStore<ApplicationUser> userStore;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager,
            ApplicationDbContext context, IUserStore<ApplicationUser> userStore)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.context = context;
            this.userStore = userStore;
        }

        public async Task<IActionResult> Index()
        {
            UpdateUserCookies(await userManager.GetUserAsync(User), Response.Cookies);
            return View((await context.Reviews.Include(r => r.Product).Include(r => r.Score).ToArrayAsync()).TakeLast(10).Reverse());
        }

        private async Task GenerateProducts()
        {
            var movies = System.IO.File.ReadAllLines("wwwroot/files/movies.txt");
            var series = System.IO.File.ReadAllLines("wwwroot/files/series.txt");
            var books = System.IO.File.ReadAllLines("wwwroot/files/books.txt");
            var games = System.IO.File.ReadAllLines("wwwroot/files/games.txt");
            for (int i = 0; i < movies.Length; i += 5)
            {
                if (!context.Movies.Any(m => m.Title == movies[i]))
                {
                    Movie m = new()
                    {
                        Title = movies[i],
                        ReleaseYear = int.Parse(movies[i + 1]),
                        Description = movies[i + 2],
                        Cover = movies[i + 3],
                        Trailer = movies[i + 4]
                    };
                    await context.Movies.AddAsync(m);
                }
            }
            for (int i = 0; i < series.Length; i += 5)
            {
                if (!context.Series.Any(s => s.Title == series[i]))
                {
                    Series s = new()
                    {
                        Title = series[i],
                        ReleaseYear = int.Parse(series[i + 1]),
                        Description = series[i + 2],
                        Cover = series[i + 3],
                        Trailer = series[i + 4]
                    };
                    await context.Series.AddAsync(s);
                }
            }
            for (int i = 0; i < games.Length; i += 5)
            {
                if (!context.Games.Any(g => g.Title == games[i]))
                {
                    Game g = new()
                    {
                        Title = games[i],
                        ReleaseYear = int.Parse(games[i + 1]),
                        Description = games[i + 2],
                        Cover = games[i + 3],
                        Trailer = games[i + 4]
                    };
                    await context.Games.AddAsync(g);
                }
            }
            for (int i = 0; i < books.Length; i += 5)
            {
                if (!context.Books.Any(g => g.Title == books[i]))
                {
                    Book b = new()
                    {
                        Title = books[i],
                        ReleaseYear = int.Parse(books[i + 1]),
                        Description = books[i + 2],
                        Cover = books[i + 3],
                        Author = books[i + 4]
                    };
                    await context.Books.AddAsync(b);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task GenerateUsers()
        {
            Random r = new();
            var names = System.IO.File.ReadAllLines("wwwroot/files/names.txt");
            var surnames = System.IO.File.ReadAllLines("wwwroot/files/surnames.txt");
            for(int i = 0; i < 0; i++)
            {
                var user = new ApplicationUser();
                var name = names[r.Next(names.Length)] + " " + surnames[r.Next(surnames.Length)];
                await userStore.SetUserNameAsync(user, name, CancellationToken.None);
                await userManager.CreateAsync(user);
            }
        }

        private async Task GenerateReviews()
        {
            for (int i = 1; i < 3; i++)
            {
                var rev = System.IO.File.ReadAllLines("wwwroot/files/reviews/" + i + ".txt");
                if (await context.Reviews.AnyAsync(r => r.Title == rev[1])) continue;
                var revBody = System.IO.File.ReadAllText("wwwroot/files/reviews/" + i + "body.txt");
                var prod = await context.Products.SingleAsync(p => p.Title == rev[0]);
                var author = await context.Users.FirstAsync(u => !u.Reviews.Any(r => r.ProductId == prod.Id));
                Review review = new()
                {
                    Title = rev[1],
                    Author = author,
                    Product = prod,
                    PublicationDate = DateTime.UtcNow,
                    Body = revBody
                };
                review.Score = new(author, review, int.Parse(rev[3]));
                foreach (var tag in rev[2].Split(",")) review.Tags.Add(new(tag, review));
                await context.Reviews.AddAsync(review);
            }
            await context.SaveChangesAsync();
        }

        private async Task GenerateScores()
        {
            var scores = await context.ProductScores.Include(s => s.Sender).ToArrayAsync();
            context.ProductScores.RemoveRange(scores);
            await context.SaveChangesAsync();

            Random r = new();
            var users = await context.Users.ToArrayAsync();
            var prods = await context.Products.ToArrayAsync();
            foreach (var user in users)
            {
                var count = r.Next(5);
                List<Product> pr = new();
                while(count > 0)
                {
                    var p = prods[r.Next(prods.Length)];
                    if (!pr.Contains(p)) pr.Add(p);
                    count--;
                }
                foreach(var p in pr)
                {
                    await context.Entry(p).Collection(p => p.UserScores).LoadAsync();
                    int score = r.Next(1, 11);
                    if (score == 1) score = r.Next(1, 3);
                    if (score < 10) score = r.Next(4, 5);
                    else score = r.Next(3, 5);
                    p.UserScores.Add(new(user, p, score));
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task GenerateLikes()
        {
            var likes = await context.ReviewLikes.Include(s => s.Sender).Include(s => s.Entity).ToArrayAsync();
            context.ReviewLikes.RemoveRange(likes);
            await context.SaveChangesAsync();

            Random rand = new();
            var users = await context.Users.ToArrayAsync();
            var reviews = await context.Reviews.ToArrayAsync();
            foreach (var user in users)
            {
                var count = rand.Next(10);
                List<Review> selected = new();
                while (count > 0)
                {
                    var rev = reviews[rand.Next(reviews.Length)];
                    if (!selected.Contains(rev)) selected.Add(rev);
                    count--;
                }
                foreach (var r in selected) r.Likes.Add(new(user, r));
            }
            await context.SaveChangesAsync();
        }

        public async Task<IActionResult> Search(string text)
        {
            string request;
            if (text.StartsWith('[') && text.EndsWith("]"))
            {
                text = text[1..(text.Length - 1)];
                request = "\"" + text + "*\" OR \"" + text + "\"";
                var res = context.ReviewTags.Where(x => EF.Functions.Contains(x.Body, request)).Include(r => r.Entity.Product).Include(r => r.Entity.Score);
                return PartialView("MainReviewsTableBody", await res.Select(t => t.Entity).ToArrayAsync());
            }
            else
            {
                request = "\"" + text + "*\" OR \"" + text + "\"";
                var byTitle = context.Reviews.Where(x => EF.Functions.Contains(x.Title, request));
                var byBody = context.Reviews.Where(x => EF.Functions.Contains(x.Body, request));
                var byProduct = context.Reviews.Where(x => EF.Functions.Contains(x.Product.Title, request));
                var byTags = context.ReviewTags.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
                var byComments = context.ReviewComments.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
                var founded = byTitle.Union(byBody).Union(byProduct).Union(byTags).Union(byComments);
                return PartialView("MainReviewsTableBody", await founded.Include(r => r.Product).Include(r => r.Score).ToArrayAsync());
            }
        }

        [HttpPost]
        public async Task<string> GetAllTags()
        {
            string res = "";
            var tags = await context.ReviewTags.Select(t => t.Body).ToListAsync();
            while(tags.Count > 0)
            {
                var tag = tags[0];
                res += tag + "," + tags.RemoveAll(t => t == tag) + ",";
            }
            return res;
        }

        [HttpPost]
        public async Task<IActionResult> GetTagList(string partTag)
        {
            var t = (await context.ReviewTags.Select(t => t.Body).ToListAsync()).Where(b => b.Contains(partTag, StringComparison.OrdinalIgnoreCase));
            return PartialView("OptionsList", t.Distinct().TakeLast(7));
        }

        [HttpPost]
        public async Task<IActionResult> GetHighScoresView()
        {
            var reviews = await context.Reviews.Include(r => r.Product).Include(r => r.Score).ToListAsync();
            var ordered = reviews.OrderBy(r => r.Score?.Value);
            return PartialView("MainReviewsTableBody", ordered.TakeLast(10).Reverse());
        }

        public async Task<IActionResult> ChangeLanguageAsync(string current, string returnUrl)
        {
            if (string.IsNullOrEmpty(current)) throw new ArgumentNullException(nameof(current));
            if (string.IsNullOrEmpty(returnUrl)) throw new ArgumentNullException(nameof(returnUrl));
            var lang = new RequestCulture(current == "en" ? "ru" : "en-US");
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(lang), new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) });
            await SaveLanguage(current == "en" ? Language.Russian : Language.English);
            return Redirect(returnUrl);
        }

        private async Task SaveLanguage(Language language)
        {
            var u = await userManager.GetUserAsync(User);
            if (u != null)
            {
                u.InterfaceLanguage = language;
                await userManager.UpdateAsync(u);
            }
        }

        [NonAction]
        public static void UpdateUserCookies(ApplicationUser? user, IResponseCookies cookies)
        {
            if (user != null)
            {
                var opt = new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) };
                var lang = new RequestCulture(user.InterfaceLanguage == Language.Russian ? "ru" : "en-US");
                cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(lang), opt);
                cookies.Append("IsDarkTheme", user.DarkTheme ? "true" : "false", opt);
            }
        }

        [HttpPost]
        public async Task SaveTheme(bool isDark)
        {
            var u = await userManager.GetUserAsync(User);
            if (u != null)
            {
                u.DarkTheme = isDark;
                await userManager.UpdateAsync(u);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}