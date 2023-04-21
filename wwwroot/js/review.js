﻿
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
            url: "/Home/GetTagList?partTag=" + el.value,
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

function checkReview() {
    if (tinymce.activeEditor.getContent().length > 10) document.getElementById("bodyIsFull").value = 1;
    else document.getElementById("bodyIsFull").value = "";
}

function changeLike(id) {
    let el = document.getElementById("likeIcon");
    let count = document.getElementById("likeCount");
    if (el.ariaLabel == "Liked") {
        el.ariaLabel = "";
        el.setAttribute("src", "/icons/thumb white.svg");
        count.textContent = Number(count.textContent) - 1;
        $.ajax({
            url: "/Review/ChangeLike?id=" + id,
            type: "POST",
            error: function (data, textStatus, jqXHR) {
                //el.ariaLabel = "Liked";
                //el.setAttribute("src", "/icons/thumb white full.svg");
                //count.textContent = Number(count.textContent) + 1;
                console.log(data.status + " | " + data.statusText);
            }
        });
    }
    else {
        el.ariaLabel = "Liked";
        el.setAttribute("src", "/icons/thumb white full.svg");
        count.textContent = Number(count.textContent) + 1;
        $.ajax({
            url: "/Review/ChangeLike?id=" + id,
            type: "POST",
            error: function (data, textStatus, jqXHR) {
                //el.ariaLabel = "";
                //el.setAttribute("src", "/icons/thumb white.svg");
                //count.textContent = Number(count.textContent) - 1;
                console.log(data.status + " | " + data.statusText);
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
        url: "/Review/AddComment?id=" + id + "&comment=" + c + "&visibleCount=" + visibleCount,
        type: "POST",
        success: function (res) {
            $("#reviewComments").html(res);
        }
    });
}

function showMoreComments(id, count) {
    $.ajax({
        url: "/Review/ShowComments?id=" + id + "&count=" + (Number(document.getElementById("visibleCount").value) + count),
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
        url: "/Review/RemoveReview/" + id,
        type: "POST",
        success: function () {
            updateReviews();
        }
    });
}
