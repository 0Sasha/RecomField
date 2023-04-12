
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
        url: "/User/GetProductsView?partTitle=" + txt,
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
}

function addTag() {
    const tag = document.getElementById("tagForm").value;
    document.getElementById("tagForm").value = "";
    if (tag != undefined && tag.length > 0) addNewTag(tag);
}

function addNewTag(tag) {
    let idTag = String(tag).replace(/\s/g, '').replace(/'/g, '').replace(/"/g, '') + "Tag";
    if (document.getElementById(idTag) != undefined) return;
    $("#tagsLine").html($("#tagsLine").html() + "<span role='button' class='badge text-bg-primary mb-1' id='" + idTag + "' onclick=deleteTag('" + idTag + "')> " + tag + "</span > ");
    let el = document.getElementById('TagsForServer');
    el.value += (el.value == "") ? tag : "," + tag;
}

function deleteTag(id) {
    document.getElementById("tagsLine").removeChild(document.getElementById(id));
    let tag = id.slice(0, id.length - 3);
    let el = document.getElementById('TagsForServer');
    let val = String(el.value);
    if (val == tag) el.value = "";
    else if (val.startsWith(tag)) el.value = val.replace(tag + ',', '');
    else el.value = val.replace(',' + tag, '');
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

function checkReview() {
    if (tinymce.activeEditor.getContent().length > 10) document.getElementById("bodyIsFull").value = 1;
    else document.getElementById("bodyIsFull").value = "";
}
