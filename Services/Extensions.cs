using Microsoft.AspNetCore.Localization;
using RecomField.Models;
namespace RecomField.Services;

public static class Extensions
{
    public static IResponseCookies AddUserCookies(this IResponseCookies cookies, ApplicationUser? user)
    {
        if (user != null)
        {
            var opt = new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) };
            var lang = new RequestCulture(user.InterfaceLanguage == Language.Russian ? "ru" : "en-US");
            cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(lang), opt);
            cookies.Append("IsDarkTheme", user.DarkTheme ? "true" : "false", opt);
        }
        return cookies;
    }
}
