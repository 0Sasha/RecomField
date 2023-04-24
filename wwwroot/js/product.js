
function changeRateProd(rate, id) {
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
    $.ajax({
        url: "/Product/ChangeScoreProduct?id=" + id + "&score=" + rate,
        type: "POST"
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

function updateProductsModal(authorId) {
    let txt = document.getElementById("searchProductsModal").value;
    if (txt == undefined) txt = "";
    $.ajax({
        url: "/Product/GetProductsForReview?authorId=" + authorId + "&partTitle=" + txt,
        type: "POST",
        dataType: "html",
        success: function (res) {
            $("#tbodyProductsModal").html(res);
        }
    });
}

function clickCoverInput() {
    document.getElementById("coverInput").click();
}

async function dropCoverInput(ev) {
    ev.preventDefault();
    if (ev.dataTransfer.items) {
        let item = ev.dataTransfer.items[0];
        if (item.kind === "file") {
            let file = item.getAsFile();
            if (validFileType(file)) {
                let url = await uploadCover(file);
                document.getElementById("coverAspInput").value = url;
                showCover(url);
            }
        }
    }
}

function dragOverHandler(ev) {
    ev.preventDefault();
}

async function setCover() {
    const coverInput = document.getElementById("coverInput");
    const curFiles = coverInput.files;
    if (curFiles.length != 0) {
        for (const file of curFiles) {
            if (validFileType(file)) {
                let url = await uploadCover(file);
                document.getElementById("coverAspInput").value = url;
                showCover(url);
            }
        }
    }
}

async function uploadCover(file) {
    let formData = new FormData();
    formData.append("file", file);
    document.getElementById("uplIcon").hidden = true;
    document.getElementById("uplText").hidden = true;
    document.getElementById("spinner").hidden = false;
    let response = await fetch('/Review/UploadImage', { method: "POST", body: formData });
    if (response.ok) {
        const jsonValue = await response.json();
        return jsonValue.location;
    }
}

function showCover(url) {
    let img = document.getElementById("coverImg");
    img.src = url;
    document.getElementById("spinner").hidden = true;
    img.hidden = false;
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
    let l = String(link);
    let startId = l.indexOf("v=") + 2;
    l = l.slice(startId);
    let endId = l.indexOf("&");
    if (endId >= 0) return l.slice(0, endId);
    return l;
}

function validFileType(file) {
    return fileTypes.includes(file.type);
}

const fileTypes = [
    "image/apng",
    "image/bmp",
    "image/gif",
    "image/jpeg",
    "image/pjpeg",
    "image/png",
    "image/svg+xml",
    "image/tiff",
    "image/webp",
    "image/x-icon"
];