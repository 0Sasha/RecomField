using iText.Layout;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using RecomField.Models;
namespace RecomField;

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
        if (link.Contains("v="))
        {
            var startId = link.IndexOf("v=") + 2;
            var endId = link.IndexOf("&", startId);
            return "https://www.youtube.com/embed/" + (endId != -1 ? link[startId..endId] : link[startId..]);
        }
        else if (link.Contains('/')) return "https://www.youtube.com/embed/" + link[(link.LastIndexOf("/") + 1)..];
        throw new ArgumentException("Id of the video is not found");
    }

    public static string CustomizeHtmlForPDF(this string body) => RemoveGaps(RemoveVideos(CustomizeImages(body)));

    private static string CustomizeImages(string body)
    {
        var startImg = body.LastIndexOf("<img");
        while (startImg >= 0)
        {
            body = RemoveAttrsEl(body, startImg, "width", "height", "style");
            var endImg = body.IndexOf(">", startImg);
            body = body.Insert(endImg, " style=\"max-width: 600px; max-height: 840px;\" ");
            startImg = body[..startImg].LastIndexOf("<img");
        }
        return body;
    }

    private static string RemoveVideos(string body)
    {
        var startVideo = body.LastIndexOf("<div class=\"ratio");
        while (startVideo >= 0)
        {
            var endVideo = body.IndexOf("</div>", startVideo);
            body = body.Remove(startVideo, endVideo - startVideo);
            startVideo = body[..startVideo].LastIndexOf("<div class=\"ratio");
        }
        return body;
    }

    private static string RemoveGaps(string body)
    {
        var endGap = body.IndexOf(">&nbsp;</p>");
        while (endGap >= 0)
        {
            var startGap = body[..endGap].LastIndexOf("<");
            body = body.Remove(startGap, endGap + 11 - startGap);
            endGap = body.IndexOf(">&nbsp;</p>");
        }
        return body;
    }

    private static string RemoveAttrsEl(string body, int startEl, params string[] attrs)
    {
        foreach (var attr in attrs)
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

    public static string CustomizeHtmlForView(this string body) => CustomizeIframe(CustomizeStyle(body));

    private static string CustomizeStyle(string body)
    {
        if (body.Contains("style=\""))
        {
            var b = body.Split("\"");
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i].EndsWith("style=") && i + 1 < b.Length)
                {
                    if (b[i + 1].StartsWith("height") || b[i + 1].StartsWith("width"))
                        b[i + 1] = b[i + 1].Insert(0, "max-");
                    b[i + 1] = b[i + 1].Replace(" width", " max-width");
                    b[i + 1] = b[i + 1].Replace(" height", " max-height");
                }
            }
            body = string.Join("\"", b);
        }
        return body;
    }

    private static string CustomizeIframe(string body)
    {
        var i = body.LastIndexOf("<iframe");
        while (i >= 0)
        {
            var boxEl = body[..i].LastIndexOf("<");
            if (body[boxEl + 1] != 'p') { i = body[..i].LastIndexOf("<iframe"); continue; }
            var j = body.IndexOf("</iframe>", i);
            body = body.Insert(j + 9, "</div>");
            var startStyle = body[..j].IndexOf("style=", i);
            var style = startStyle != -1 ? body[startStyle..(body.IndexOf("\"", startStyle + 7) + 1)] : "";
            if (style != "") body = body.Remove(startStyle, style.Length);
            body = body.Insert(i, " <div class=\"ratio ratio-16x9\" " + style + "> ");
            i = body[..i].LastIndexOf("<iframe");
        }
        return body;
    }

    public static string ReverseCustomizedHtml(this string body)
    {
        var i = body.LastIndexOf("<iframe");
        while (i >= 0)
        {
            var startBoxEl = body[..i].LastIndexOf("<div");
            var startStyle = body.IndexOf("style=", startBoxEl, i - startBoxEl);
            var style = startStyle != -1 ? body[startStyle..(body.IndexOf("\"", startStyle + 7) + 1)] : "";
            if (style != "") body = body.Insert(i + 7, " " + style + " ");
            body = body.Remove(startBoxEl, body.IndexOf(">", startBoxEl) - startBoxEl + 1);
            body = body.Insert(startBoxEl, "<p>");
            var endBoxEl = body.IndexOf("</div>", startBoxEl);
            body = body.Remove(endBoxEl, 6);
            body = body.Insert(endBoxEl, "</p>");
            i = body.LastIndexOf("<iframe", startBoxEl);
        }
        return body;
    }
}
