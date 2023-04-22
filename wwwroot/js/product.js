
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
