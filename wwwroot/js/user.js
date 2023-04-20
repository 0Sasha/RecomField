
function blockUser(id, days) {
    let partUrl = days == undefined ? "" : "&days=" + days;
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
        url: "/User/UnlockUser?id=" + id,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function removeUser(id) {
    $.ajax({
        url: "/User/RemoveUser?id=" + id,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function addAdminRole(id) {
    $.ajax({
        url: "/User/AddAdminRole?id=" + id,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}

function revokeAdminRole(id) {
    $.ajax({
        url: "/User/RevokeAdminRole?id=" + id,
        type: "POST",
        success: function (res) {
            $("#tbodyUsers").html(res);
        }
    });
}
