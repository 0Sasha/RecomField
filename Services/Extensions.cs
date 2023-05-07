using Microsoft.AspNetCore.Localization;
using RecomField.Models;
namespace RecomField.Services;

public static class Extensions
{
    public static void AddUserCookies(this IResponseCookies cookies, ApplicationUser? user)
    {
        if (cookies == null) throw new ArgumentNullException(nameof(cookies));
        if (user != null)
        {
            var opt = new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) };
            var lang = new RequestCulture(user.InterfaceLanguage == Language.Russian ? "ru" : "en-US");
            cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(lang), opt);
            cookies.Append("IsDarkTheme", user.DarkTheme.ToString(), opt);
        }
    }

    public static string CustomizeYouTubeLink(this string link)
    {
        if (string.IsNullOrEmpty(link)) throw new ArgumentNullException(nameof(link));
        var startId = link.IndexOf("v=");
        if (startId == -1) throw new ArgumentException("Id of video is not found");
        startId += 2;
        var endId = link.IndexOf("&", startId);
        return "https://www.youtube.com/embed/" + (endId >= 0 ? link[startId..endId] : link[startId..]);
    }
}
