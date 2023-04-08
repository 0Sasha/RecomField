
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
    let isDark = "true";
    if (document.documentElement.getAttribute("data-bs-theme") == "light") {
        document.documentElement.setAttribute("data-bs-theme", "dark");
        document.getElementById("themeIcon").setAttribute("src", "/icons/sun.svg");
        document.getElementById("globeIcon").setAttribute("src", "/icons/globe.svg");
    }
    else {
        isDark = "false";
        document.documentElement.setAttribute("data-bs-theme", "light");
        document.getElementById("themeIcon").setAttribute("src", "/icons/dark sun.svg");
        document.getElementById("globeIcon").setAttribute("src", "/icons/dark globe.svg");
    }
    const d = new Date();
    d.setDate(d.getDate() + 30);
    document.cookie = "IsDarkTheme=" + isDark + "; expires=" + d.toUTCString() + "; path=/";
    $.ajax({
        url: "Home/SaveTheme?isDark=" + isDark,
        type: "POST",
    });
}
