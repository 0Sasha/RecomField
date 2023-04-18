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

function updateProductsModal() {
    let txt = document.getElementById("searchProductsModal").value;
    if (txt == undefined) txt = "";
    $.ajax({
        url: "/User/GetProductsForReview?partTitle=" + txt,
        type: "POST",
        dataType: "html",
        success: function (res) {
            $("#tbodyProductsModal").html(res);
        }
    });
}

function selectProduct(id, type, title, release) {
    document.getElementById("ProductIdForServer").value = id;
    document.getElementById("typeProd").value = type;
    document.getElementById("titleProd").value = title;
    document.getElementById("releaseProd").value = release;
    clearTags();
    addNewTag(release);
}

function checkTag() {
    let el = document.getElementById("tagForm");
    if (el.value[el.value.length - 1] == ',') {
        const tag = el.value.replace(',', '');
        el.value = "";
        addNewTag(tag);
    }
    else if (el.value.length > 0) {
        $.ajax({
            url: "/User/GetTagList?partTag=" + el.value,
            type: "POST",
            success: function (res) {
                $("#tagsList").html(res);
            }
        });
    }
}

function addTag() {
    const tag = document.getElementById("tagForm").value;
    document.getElementById("tagForm").value = "";
    if (tag != undefined && tag.length > 0) addNewTag(tag);
}

function addNewTag(tag) {
    let idTag = String(tag).replace(/\s/g, '').replace(/'/g, '').replace(/"/g, '') + "Tag";
    if (document.getElementById(idTag) != undefined) return;
    $("#tagsLine").html($("#tagsLine").html() + "<span role='button' class='badge text-bg-primary mb-1' id='" + idTag + "' onclick=deleteTag('" + idTag + "')>" + tag + "</span > ");
    let el = document.getElementById('TagsForServer');
    el.value += (el.value == "") ? tag : "," + tag;
}

function addAllTags(tags) {
    let code = "";
    String(tags).split(',').forEach(t => code += "<span class='badge text-bg-primary mb-1'>" + t + "</span > ");
    $("#tagsLine").html(code);
}

function deleteTag(id) { // Check ////////////////////////////////////////////
    let e = document.getElementById(id);
    let fullTag = e.textContent;
    document.getElementById("tagsLine").removeChild(e);
    let el = document.getElementById('TagsForServer');
    let val = String(el.value);
    if (val == fullTag) el.value = "";
    else if (val.startsWith(fullTag)) el.value = val.replace(fullTag + ',', '');
    else el.value = val.replace(',' + fullTag, '');
}

function clearTags() {
    const e = document.getElementById("tagsLine");
    while (e.children.length > 0) {
        e.removeChild(e.children[0]);
    }
    document.getElementById('TagsForServer').value = "";
}

function changeRate(rate) {
    var el = document.getElementsByName("rateBtn");
    for (var i = 1; i <= 10; i++) {
        if (i <= rate) {
            el[i - 1].classList.remove('btn-light');
            el[i - 1].classList.add('btn-warning');
        }
        else {
            el[i - 1].classList.remove('btn-warning');
            el[i - 1].classList.add('btn-light');
        }
    }
    document.getElementById("myRating").textContent = "Rating: " + rate + "/10";
    document.getElementById("RateForServer").value = rate;
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

function checkReview() {
    if (tinymce.activeEditor.getContent().length > 10) document.getElementById("bodyIsFull").value = 1;
    else document.getElementById("bodyIsFull").value = "";
}

function changeLike(id) {
    let el = document.getElementById("likeIcon");
    if (el.ariaLabel == "Liked") {
        $.ajax({
            url: "/User/ChangeLike?id=" + id,
            type: "POST",
            success: function () {
                el.setAttribute("src", "/icons/thumb white.svg");
                el.ariaLabel = "";
            }
        });
    }
    else {
        $.ajax({
            url: "/User/ChangeLike?id=" + id,
            type: "POST",
            success: function () {
                el.setAttribute("src", "/icons/thumb white full.svg");
                el.ariaLabel = "Liked";
            }
        });
    }
}

function addComment(id, visibleCount) {
    let el = document.getElementById("comment");
    if (el.value.length == 0) return;
    let c = el.value;
    el.value = "";
    $.ajax({
        url: "/User/AddComment?id=" + id + "&comment=" + c + "&visibleCount=" + visibleCount,
        type: "POST",
        success: function (res) {
            $("#reviewComments").html(res);
        }
    });
}

function showMoreComments(id, count) {
    $.ajax({
        url: "/User/ShowComments?id=" + id + "&count=" + (Number(document.getElementById("visibleCount").value) + count),
        type: "POST",
        success: function (res) {
            $("#reviewComments").html(res);
        }
    });
}

function updateReviewComments(id) {
    if (window.location.href.includes("Review/" + id) || window.location.href.includes("Review?id=" + id)) {
        showMoreComments(id, 1);
    }
}

function removeReview(id) {
    $.ajax({
        url: "/User/RemoveReview/" + id,
        type: "POST",
        success: function () {
            updateReviews();
        }
    });
}

function updateReviews() {
    let search = document.getElementById("searchReview").value;
    search = search.length > 0 ? "?search=" + search : "";
    $.ajax({
        url: "/User/GetReviewsView" + search,
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
        if ($('#dropdownMenu').is(":hidden")) {
            $('#searchLine').dropdown('toggle');
        }
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