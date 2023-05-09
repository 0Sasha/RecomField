
const image_upload_handler = (blobInfo, progress) => new Promise((resolve, reject) => {
    const request = new XMLHttpRequest();
    request.withCredentials = false;
    request.open('POST', '/Review/UploadImage', true);

    request.upload.onprogress = (e) => {
        progress(e.loaded / e.total * 100);
    };

    request.onload = () => {
        if (request.status === 403) {
            reject({ message: 'HTTP Error: ' + request.status, remove: true });
            return;
        }

        if (request.status < 200 || request.status >= 300) {
            reject('HTTP Error: ' + request.status);
            return;
        }

        if (request.responseText.startsWith("Error")) {
            reject(request.responseText);
            tinymce.execCommand('Undo');
            return;
        }
        const json = JSON.parse(request.responseText);

        if (!json || typeof json.location != 'string') {
            reject('Invalid JSON: ' + request.responseText);
            return;
        }

        resolve(json.location);
    };

    request.onerror = () => {
        reject('Image upload failed due to a request Transport error. Code: ' + request.status);
    };

    const formData = new FormData();
    formData.append('file', blobInfo.blob(), blobInfo.filename());

    request.send(formData);
});

tinymce.init({
    selector: 'textarea#tiny',
    language: siteLanguage,
    images_upload_handler: image_upload_handler,
    extended_valid_elements: 'img[class=img-fluid|src|alt|width|height]',
    plugins: [
        'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
        'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
        'insertdatetime', 'media', 'table', 'help', 'wordcount'
    ],
    init_instance_callback: (editor) => {
        checkReview(false);
        editor.on('Change', (e) => {
            console.log(`The editor content changes have been committed.`);
            checkReview(true);
        });
    }
});

function addAllTags(tags) {
    let code = "";
    String(tags).split(',').forEach(t => code += "<span class='badge text-bg-primary mb-1'>" + t + "</span > ");
    $("#tagsLine").html(code);
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
            url: "/Review/GetTagList?partTag=" + el.value,
            type: "POST",
            success: function (res) {
                $("#tagsList").html(res);
            }
        });
    }
}

let idTag = 1;
let allTags = [];

function addNewTag(tag) {
    if (tag.includes(",") || allTags.includes(tag)) return;
    let id = "unIdTag" + idTag;
    idTag++;
    $("#tagsLine").html($("#tagsLine").html() + "<span role='button' class='badge text-bg-primary mb-1' id='" + id + "' onclick=deleteTag('" + id + "')>" + tag + "</span > ");
    allTags.push(tag);
    validateTags();
}

function deleteTag(id) {
    let e = document.getElementById(id);
    let tag = e.textContent;
    document.getElementById("tagsLine").removeChild(e);
    allTags.splice(allTags.indexOf(tag), 1);
    validateTags();
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
    let rt = document.getElementById("myRating");
    rt.textContent = rt.textContent.slice(0, rt.textContent.indexOf(":")) + ": " + rate + "/10";
    document.getElementById("RateForServer").value = rate;
    validateRate();
}

function validate() {
    validateTags();
    validateBody();
    validateRate();
}

function validateTags() {
    let el = document.getElementById('TagsForServer');
    el.value = allTags.join(',');
    document.getElementById("tagFeedback").hidden = el.value != "";
}

function validateRate() {
    document.getElementById("rateFeedback").hidden = document.getElementById("RateForServer").value != "";
}

function validateBody() {
    document.getElementById("bodyFeedback").hidden = tinymce.activeEditor.getContent().length > 10;
}

function checkReview(feedback) {
    if (tinymce.activeEditor.getContent().length > 10) document.getElementById("bodyIsFull").value = 1;
    else document.getElementById("bodyIsFull").value = "";
    if (feedback) validateBody();
}

function changeLike(id) {
    let el = document.getElementById("likeIcon");
    let count = document.getElementById("likeCount");
    if (el.ariaLabel == "Liked") {
        el.ariaLabel = "";
        el.setAttribute("src", "/icons/thumb white.svg");
        count.textContent = Number(count.textContent) - 1;
    }
    else {
        el.ariaLabel = "Liked";
        el.setAttribute("src", "/icons/thumb white full.svg");
        count.textContent = Number(count.textContent) + 1;
    }
    $.ajax({
        url: "/Review/ChangeLike?id=" + id,
        type: "POST"
    });
}

function addComment(id, visibleCount) {
    let el = document.getElementById("comment");
    if (el.value.length == 0) return;
    let c = el.value;
    el.value = "";
    $.ajax({
        url: "/Review/AddComment?id=" + id + "&comment=" + c + "&visibleCount=" + visibleCount,
        type: "POST",
        success: function (res) {
            $("#reviewComments").html(res);
        }
    });
}

function showMoreComments(id, count) {
    let el = document.getElementById("visibleCount");
    if (el != undefined) count += Number(el.value)
    $.ajax({
        url: "/Review/ShowComments?id=" + id + "&count=" + count,
        type: "POST",
        success: function (res) {
            $("#reviewComments").html(res);
        }
    });
}

function updateReviewComments(id) {
    if (window.location.href.includes("Review/Index/" + id) || window.location.href.includes("Review/Index?id=" + id)) {
        showMoreComments(id, 1);
    }
}

function removeReview(id) {
    $.ajax({
        url: "/Review/RemoveReview/" + id,
        type: "POST",
        success: function () {
            updateReviews();
        }
    });
}

function loadSimilarReviews(id) {
    $.ajax({
        url: "/Review/GetSimilarReviews?id=" + id,
        type: "POST",
        success: function (res) {
            if (res.length > 10) {
                $("#tbodyReviews").html(res);
                document.getElementById("similarDiv").hidden = false;
            }
        }
    });
}
