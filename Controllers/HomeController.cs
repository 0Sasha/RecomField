using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace RecomField.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<HomeController> logger;
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            UpdateUserCookies(await userManager.GetUserAsync(User), Response.Cookies);
            return View((await context.Review.Include(r => r.Product).Include(r => r.Score).ToArrayAsync()).TakeLast(10).Reverse());
        }

        public async Task<IActionResult> Search(string text)
        {
            string request;
            if (text.StartsWith('[') && text.EndsWith("]"))
            {
                text = text[1..(text.Length - 1)];
                request = "\"" + text + "*\" OR \"" + text + "\"";
                var res = context.ReviewTag.Where(x => EF.Functions.Contains(x.Body, request)).Include(r => r.Entity.Product).Include(r => r.Entity.Score);
                return PartialView("MainReviewsTableBody", await res.Select(t => t.Entity).ToArrayAsync());
            }
            else
            {
                request = "\"" + text + "*\" OR \"" + text + "\"";
                var byTitle = context.Review.Where(x => EF.Functions.Contains(x.Title, request));
                var byBody = context.Review.Where(x => EF.Functions.Contains(x.Body, request));
                var byProduct = context.Review.Where(x => EF.Functions.Contains(x.Product.Title, request));
                var byTags = context.ReviewTag.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
                var byComments = context.ReviewComment.Where(x => EF.Functions.Contains(x.Body, request)).Select(t => t.Entity);
                var founded = byTitle.Union(byBody).Union(byProduct).Union(byTags).Union(byComments);
                return PartialView("MainReviewsTableBody", await founded.Include(r => r.Product).Include(r => r.Score).ToArrayAsync());
            }
        }

        [HttpPost]
        public async Task<string> GetAllTags()
        {
            string res = "";
            var tags = await context.ReviewTag.Select(t => t.Body).ToListAsync();
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
            var t = (await context.ReviewTag.Select(t => t.Body).ToListAsync()).Where(b => b.Contains(partTag, StringComparison.OrdinalIgnoreCase));
            return PartialView("OptionsList", t.Distinct().TakeLast(7));
        }

        [HttpPost]
        public async Task<IActionResult> GetHighScoresView()
        {
            var reviews = await context.Review.Include(r => r.Product).Include(r => r.Score).ToListAsync();
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