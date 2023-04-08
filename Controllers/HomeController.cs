﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
using System.Diagnostics;
using System.Globalization;

namespace RecomField.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            //foreach(var u in _userManager.Users.ToArray())
            //    await _userManager.DeleteAsync(u);
            return View((object)Program.Environment);
        }

        public IActionResult ChangeLanguage(string current, string returnUrl)
        {
            if (string.IsNullOrEmpty(current)) throw new ArgumentNullException(nameof(current));
            if (string.IsNullOrEmpty(returnUrl)) throw new ArgumentNullException(nameof(returnUrl));
            var lang = new RequestCulture(current == "en" ? "ru" : "en-US");
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(lang), new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) });
            _ = SaveLanguage(current == "en" ? Language.Russian : Language.English);
            return Redirect(returnUrl);
        }

        private async Task SaveLanguage(Language language)
        {
            var u = await _userManager.GetUserAsync(User);
            if (u != null)
            {
                u.InterfaceLanguage = language;
                await _userManager.UpdateAsync(u);
            }
        }

        [HttpPost]
        public async Task SaveTheme(bool isDark)
        {
            var u = await _userManager.GetUserAsync(User);
            if (u != null)
            {
                u.DarkTheme = isDark;
                await _userManager.UpdateAsync(u);
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