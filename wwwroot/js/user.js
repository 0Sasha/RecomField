
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
