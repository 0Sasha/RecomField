
if (getCookie("IsDarkTheme").toLowerCase() == "false")
    document.documentElement.setAttribute("data-bs-theme", "light");

function getCookie(name) {
    return document.cookie.split('; ').find((row) => row.startsWith(name + "="))?.split("=")[1];
}

function changeTheme() {
    let isDark = true;
    if (document.documentElement.getAttribute("data-bs-theme") == "light")
        document.documentElement.setAttribute("data-bs-theme", "dark");
    else {
        isDark = false;
        document.documentElement.setAttribute("data-bs-theme", "light");
    }
    document.getElementById("themeIcon").setAttribute("src", isDark ? "/icons/sun.svg" : "/icons/dark sun.svg");
    document.getElementById("globeIcon").setAttribute("src", isDark ? "/icons/globe.svg" : "/icons/dark globe.svg");
    const d = new Date();
    d.setDate(d.getDate() + 30);
    document.cookie = "IsDarkTheme=" + isDark + "; expires=" + d.toUTCString() + "; path=/";
    $.ajax({
        url: "/Home/SaveTheme?isDark=" + isDark,
        type: "POST"
    });
}


function searchReviewsByTag(text) {
    if (text.length > 0) {
        document.getElementById('typeSearch').value = 'false';
        document.getElementById('searchLine').value = text;
        updateSearchList();
    }
}

function updateSearchList() {
    let text = document.getElementById('searchLine').value;
    if (text.length > 0) sendSearchRequest(text, document.getElementById('typeSearch').value);
    else if (!$('#dropdownMenu').is(":hidden")) $('#searchLine').dropdown('toggle');
}

let curSearchRequest = undefined;

function sendSearchRequest(text, isProds) {
    if (curSearchRequest != undefined) curSearchRequest.abort();
    curSearchRequest = $.ajax({
        url: "/Home/Search?text=" + text + "&products=" + isProds,
        type: "POST",
        success: function (res) {
            $('#tbodySearch').html(res);
            if ($('#dropdownMenu').is(":hidden")) $('#searchLine').dropdown('toggle');
            curSearchRequest = undefined;
        },
        error: function (jqXHR, exception) { curSearchRequest = undefined; }
    });
    if ($('#navbarCollapse').is(":hidden")) $('#navbarCollapse').collapse('toggle');
    if ($('#dropdownMenu').is(":hidden")) $('#searchLine').dropdown('toggle');
}

let siteLanguage = getCookie(".AspNetCore.Culture");
if (siteLanguage != undefined && siteLanguage.endsWith("ru")) siteLanguage = "ru";
else siteLanguage = "en-US";
