let curReqUpdating = undefined;

function updateReviews() {
    if (curReqUpdating != undefined) curReqUpdating.abort();
    let search = document.getElementById("searchReview");
    let sortedSel = document.getElementById("sortedSel");
    curReqUpdating = $.ajax({
        url: "/User/GetReviewsView?userId=" + search.attributes.userId.value + "&search=" + search.value +
            "&sort=" + sortedSel.value + "&ascOrder=" + sortedSel.ascOrder,
        type: "POST",
        success: function (res) {
            $("#tbodyReviews").html(res);
            curReqUpdating = undefined;
        },
        error: function (jqXHR, exception) { curReqUpdating = undefined; }
    });
}

function updateUsers() {
    if (curReqUpdating != undefined) curReqUpdating.abort();
    let search = document.getElementById("searchUser").value;
    let typeUsers = document.getElementById("typeUsers").value;
    let countUsers = document.getElementById("countUsers").value;
    curReqUpdating = $.ajax({
        url: "/User/GetUsersView?search=" + search + "&type=" + typeUsers + "&count=" + countUsers,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
            curReqUpdating = undefined;
        },
        error: function (jqXHR, exception) { curReqUpdating = undefined; }
    });
}

function changeOrderSort() {
    let sortedSel = document.getElementById("sortedSel");
    sortedSel.ascOrder = sortedSel.ascOrder == "true" ? "false" : "true";
    updateReviews();
}

function blockUser(id, days) {
    $.ajax({
        url: "/User/BlockUser?id=" + id + "&days=" + days,
        type: "POST",
        success: function (res) {
            updateUsers();
        }
    });
}

function unblockUser(id) {
    $.ajax({
        url: "/User/UnlockUser?id=" + id,
        type: "POST",
        success: function (res) {
            updateUsers();
        }
    });
}

function removeUser(id) {
    $.ajax({
        url: "/User/RemoveUser?id=" + id,
        type: "POST",
        success: function (res) {
            updateUsers();
        }
    });
}

function addAdminRole(id) {
    $.ajax({
        url: "/User/AddAdminRole?id=" + id,
        type: "POST",
        success: function (res) {
            updateUsers();
        }
    });
}

function revokeAdminRole(id) {
    $.ajax({
        url: "/User/RevokeAdminRole?id=" + id,
        type: "POST",
        success: function (res) {
            updateUsers();
        }
    });
}
