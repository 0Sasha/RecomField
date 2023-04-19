﻿
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


function changeRateProd(rate, id) {
    $.ajax({
        url: "/User/ChangeScoreProduct?id=" + id + "&score=" + rate,
        type: "POST",
        success: function () {
            var el = document.getElementsByName("rateBtn");
            for (var i = 1; i <= 5; i++) {
                if (i <= rate) {
                    el[i - 1].classList.remove('btn-light');
                    el[i - 1].classList.add('btn-warning');
                }
                else {
                    el[i - 1].classList.remove('btn-warning');
                    el[i - 1].classList.add('btn-light');
                }
            }
        }
    });
}

function setInitRateProd(rate) {
    var el = document.getElementsByName("rateBtn");
    for (var i = 1; i <= 5; i++) {
        if (i <= rate) {
            el[i - 1].classList.remove('btn-light');
            el[i - 1].classList.add('btn-warning');
        }
        else {
            el[i - 1].classList.remove('btn-warning');
            el[i - 1].classList.add('btn-light');
        }
    }
}

function updateReviews(id) {
    let partURL = id == undefined || id.length == 0 ? "" : "?id=" + id;
    let search = document.getElementById("searchReview").value;
    if (search.length > 0) partURL += (partURL.length > 0 ? "&search=" : "?search=") + search;
    $.ajax({
        url: "/User/GetReviewsView" + partURL,
        type: "POST",
        success: function (res) {
            $("#tbodyReviews").html(res);
        }
    });
}

function sortUserReviews(col) {
    var table, rows, sorting, i, x, y;
    table = document.getElementById("userReviews");
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

function blockUser(id, days) {
    let partUrl = days == undefined ? "" : "&days=" + days;
    $.ajax({
        url: "/User/BlockUser?id=" + id + partUrl,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function unblockUser(id) {
    $.ajax({
        url: "/User/UnlockUser?id=" + id,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function removeUser(id) {
    $.ajax({
        url: "/User/RemoveUser?id=" + id,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function addAdminRole(id) {
    $.ajax({
        url: "/User/AddAdminRole?id=" + id,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function revokeAdminRole(id) {
    $.ajax({
        url: "/User/RevokeAdminRole?id=" + id,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function search(text) {
    if (text.length > 0) {
        document.getElementById('searchLine').value = text;
        updateSearchList();
    }
}

function updateSearchList() {
    let text = document.getElementById('searchLine').value;
    if (text.length > 0) {
        $.ajax({
            url: "/Home/Search?text=" + text,
            type: "POST",
            async: true,
            success: function (res) {
                $('#tbodySearch').html(res);
            }
        });
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

function forbidHidingSearchList() {
    canHideSearchList = false;
}

function allowHidingSearchList() {
    canHideSearchList = true;
}