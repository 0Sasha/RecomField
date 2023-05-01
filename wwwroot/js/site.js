
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
        url: "/Home/SaveTheme?isDark=" + isDark,
        type: "POST",
    });
}


function sortTable(col, idTable) {
    var table, rows, sorting, i, x, y;
    table = document.getElementById(idTable);
    sorting = true;
    while (sorting) {
        sorting = false;
        rows = table.rows;
        for (i = 1; i < rows.length - 1; i++) {
            x = rows[i].getElementsByTagName("TD")[col];
            y = rows[i + 1].getElementsByTagName("TD")[col];
            if (table.dirSort == "asc") {
                if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {
                    rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
                    sorting = true;
                    break;
                }
            }
            else if (x.innerHTML.toLowerCase() < y.innerHTML.toLowerCase()) {
                rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
                sorting = true;
                break;
            }
        }
    }
    table.dirSort = table.dirSort == "asc" ? "desc" : "asc";
}

function filterTable(idbodyTable, idSearchInput, urlMethod) {
    $.ajax({
        url: urlMethod + "?text=" + document.getElementById(idSearchInput).value,
        type: "POST",
        success: function (res) {
            $("#" + idbodyTable).html(res);
        }
    });
}


function searchReviewsByTag(text) {
    if (text.length > 0) {
        document.getElementById('searchLine').value = text;
        document.getElementById('typeSearch').value = '1';
        $("#searchLine").focus();
        updateSearchList();
    }
}

function updateSearchList() {
    let text = document.getElementById('searchLine').value;
    let type = document.getElementById('typeSearch').value == '0';
    if (text.length > 0) {
        $.ajax({
            url: "/Home/Search?text=" + text + "&products=" + type,
            type: "POST",
            async: true,
            success: function (res) {
                $('#tbodySearch').html(res);
            }
        });
        canHideSearchList = true;
        if ($('#navbarCollapse').is(":hidden")) $('#navbarCollapse').collapse('toggle');
        if ($('#dropdownMenu').is(":hidden")) $('#searchLine').dropdown('toggle');
    }
    else hideSearchList();
}

let canHideSearchList = true;

function hideSearchList() {
    if (canHideSearchList && !$('#dropdownMenu').is(":hidden"))
        $('#searchLine').dropdown('toggle');
}

function stronglyHideSearchList() {
    canHideSearchList = true;
    hideSearchList();
}

function forbidHidingSearchList() {
    canHideSearchList = false;
}

function allowHidingSearchList() {
    canHideSearchList = true;
    if (!$('#searchLine').is(":focus")) hideSearchList();
}