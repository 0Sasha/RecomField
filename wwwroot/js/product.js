
function changeRateProd(rate, id) {
    setRateProd(rate);
    $.ajax({
        url: "/Product/ChangeScoreProduct?id=" + id + "&score=" + rate,
        type: "POST"
    });
}

function setRateProd(rate) {
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

let curSearchProdsRequest = undefined;

function updateProductsModal(authorId) {
    let txt = document.getElementById("searchProductsModal").value;
    if (txt == undefined) txt = "";
    if (curSearchProdsRequest != undefined) curSearchProdsRequest.abort();
    curSearchProdsRequest = $.ajax({
        url: "/Product/GetProductsForReview?authorId=" + authorId + "&partTitle=" + txt,
        type: "POST",
        success: function (res) {
            $("#tbodyProductsModal").html(res);
            curSearchProdsRequest = undefined;
        },
        error: function (jqXHR, exception) { curSearchProdsRequest = undefined; }
    });
}

function clickCoverInput() {
    document.getElementById("coverInput").click();
}

function dragOverHandler(ev) {
    ev.preventDefault();
}

function dropCoverInput(ev) {
    ev.preventDefault();
    if (ev.dataTransfer.items) {
        let item = ev.dataTransfer.items[0];
        if (item.kind === "file") tryUploadCover(item.getAsFile());
    }
}

function setCover() {
    const coverInput = document.getElementById("coverInput");
    if (coverInput.files.length != 0) tryUploadCover(coverInput.files[0]);
}

async function tryUploadCover(file) {
    if (validFileType(file)) {
        if (document.getElementById("coverAspInput").value != "") removeCover();
        let url = await uploadCover(file);
        if (url != undefined) {
            document.getElementById("coverAspInput").value = url;
            showCover(url);
            return;
        }
    }
    else console.log("invalidFileType");
    document.getElementById("coverFeedback").hidden = false;
}

async function uploadCover(file) {
    let formData = new FormData();
    formData.append("file", file);
    startSpinner();
    let response = await fetch('/Home/UploadImage', { method: "POST", body: formData });
    let res = await response.json();
    if (res.location != undefined) return res.location;
    cancelSpinner();
    if (res.error != undefined) console.log(res.error);
}

function startSpinner() {
    document.getElementById("uplIcon").hidden = true;
    document.getElementById("uplText").hidden = true;
    document.getElementById("coverFeedback").hidden = true;
    document.getElementById("spinner").hidden = false;
}

function cancelSpinner() {
    document.getElementById("spinner").hidden = true;
    document.getElementById("uplIcon").hidden = false;
    document.getElementById("uplText").hidden = false;
}

function showCover(url) {
    let img = document.getElementById("coverImg");
    img.src = url;
    hideCoverElements();
    img.hidden = false;
}

function removeCover() {
    let img = document.getElementById("coverImg");
    img.hidden = true;
    document.getElementById("coverAspInput").value = "";
    const coverInput = document.getElementById("coverInput");
    coverInput.hidden = false;
    if (!coverInput.hasAttribute("required")) coverInput.setAttribute("required", "");
}

function hideCoverElements() {
    const coverInput = document.getElementById("coverInput");
    coverInput.hidden = true;
    coverInput.removeAttribute("required");
    document.getElementById("coverFeedback").hidden = true;
    document.getElementById("spinner").hidden = true;
}

function showTrailer() {
    let link = document.getElementById("linkToTrailer").value;
    if (link.length > 0) {
        let id = getIdVideo(link);
        document.getElementById("iframeTrailer").src = "https://www.youtube.com/embed/" + id;
        document.getElementById("divTrailer").hidden = false;
    }
    else document.getElementById("divTrailer").hidden = true;
}

function getIdVideo(link) {
    if (link.includes("v=")) link = link.slice(link.indexOf("v=") + 2);
    else if (link.includes("/")) return link.slice(link.lastIndexOf("/") + 1);
    else return "";
    let endId = link.indexOf("&");
    if (endId != -1) return link.slice(0, endId);
    return link;
}

function validFileType(file) {
    return fileTypes.includes(file.type);
}

const fileTypes = [
    "image/apng",
    "image/jpeg",
    "image/pjpeg",
    "image/png"
];
