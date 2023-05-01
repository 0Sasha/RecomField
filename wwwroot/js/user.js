
function updateReviews() {
    let search = document.getElementById("searchReview");
    let sortedSel = document.getElementById("sortedSel");
    $.ajax({
        url: "/User/GetReviewsView?userId=" + search.attributes.userId.value + "&search=" + search.value +
            "&sort=" + sortedSel.value + "&ascOrder=" + sortedSel.ascOrder,
        type: "POST",
        success: function (res) {
            $("#tbodyReviews").html(res);
        }
    });
}

function changeOrderSort() {
    let sortedSel = document.getElementById("sortedSel");
    sortedSel.ascOrder = sortedSel.ascOrder == "true" ? "false" : "true";
    updateReviews();
}

function blockUser(id, days) {
    let partUrl = (days == undefined ? "" : "&days=" + days) + getFilterUrl();
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
        url: "/User/UnlockUser?id=" + id + getFilterUrl(),
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function removeUser(id) {
    $.ajax({
        url: "/User/RemoveUser?id=" + id + getFilterUrl(),
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function addAdminRole(id) {
    $.ajax({
        url: "/User/AddAdminRole?id=" + id + getFilterUrl(),
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function revokeAdminRole(id) {
    $.ajax({
        url: "/User/RevokeAdminRole?id=" + id + getFilterUrl(),
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function getFilterUrl() {
    let filter = document.getElementById("searchInput").value;
    return filter.length > 0 ? "&filter=" + filter : "";
}
