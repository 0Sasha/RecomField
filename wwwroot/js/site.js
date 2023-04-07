
if (getCookie("IsDarkTheme") == "false") {
    document.documentElement.setAttribute("data-bs-theme", "light");
}

function getCookie(name) {
    const nm = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nm) == 0) return c.substring(nm.length, c.length);
    }
    return null;
}

function changeTheme() {
    if (document.documentElement.getAttribute("data-bs-theme") == "light") {
        document.documentElement.setAttribute("data-bs-theme", "dark");
        document.getElementById("themeIcon").setAttribute("src", "/lib/icons/sun.svg");
        document.cookie = "IsDarkTheme=true; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/";
    }
    else {
        document.documentElement.setAttribute("data-bs-theme", "light");
        document.getElementById("themeIcon").setAttribute("src", "/lib/icons/dark sun.svg");
        const d = new Date();
        d.setDate(d.getDate() + 2);
        document.cookie = "IsDarkTheme=false; expires=" + d.toUTCString() + "; path=/";
    }
}