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

function changeUser(id, method, nameArg, valArg) {
    let addPart = nameArg != undefined ? "&" + nameArg + "=" + valArg : "";
    $.ajax({
        url: "/User/" + method + "?id=" + id + addPart,
        type: "POST",
        success: function (res) {
            updateUsers();
        }
    });
}
