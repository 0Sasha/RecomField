
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
                return;
            }
        }
        document.getElementById("coverFeedback").hidden = false;
    }
}

async function setCover() {
    const coverInput = document.getElementById("coverInput");
    if (coverInput.files.length != 0) {
        const file = coverInput.files[0];
        if (validFileType(file)) {
            let url = await uploadCover(file);
            document.getElementById("coverAspInput").value = url;
            showCover(url);
        }
        else document.getElementById("coverFeedback").hidden = false;
    }
}

async function uploadCover(file) {
    let formData = new FormData();
    formData.append("file", file);
    startSpinner();
    let response = await fetch('/Home/UploadImage', { method: "POST", body: formData });
    if (response.ok) {
        const jsonValue = await response.json();
        return jsonValue.location;
    }
    else cancelSpinner();
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
    "image/jpeg",
    "image/pjpeg",
    "image/png"
];
