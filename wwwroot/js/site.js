
function changeTheme() {
    if (document.documentElement.getAttribute('data-bs-theme') == "light") {
        document.documentElement.setAttribute('data-bs-theme', 'dark');
        document.getElementById("themeIcon").setAttribute('src', "/lib/icons/sun.svg");
    }
    else {
        document.documentElement.setAttribute('data-bs-theme', 'light');
        document.getElementById("themeIcon").setAttribute('src', "/lib/icons/dark sun.svg");
    }
}