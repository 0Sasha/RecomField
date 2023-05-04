
$.ajax({
    url: "/Home/GetAllTags",
    type: "POST",
    success: function (tags) {
        let t = tags.split(',');
        if (t.length > 5) initTagCloud(t);
    }
});

$.ajax({
    url: "/Home/GetNewReviewsView",
    type: "POST",
    success: function (res) {
        $("#tbodyNewReviews").html(res);
    }
});

$.ajax({
    url: "/Home/GetMostLikedView",
    type: "POST",
    success: function (res) {
        $("#tbodyLikedReviews").html(res);
    }
});

$.ajax({
    url: "/Home/GetHighScoresView",
    type: "POST",
    success: function (res) {
        $("#tbodyReviews").html(res);
    }
});

function initTagCloud(tags) {
    var chart = am4core.create("tagCloud", am4plugins_wordCloud.WordCloud);
    //chart.fontFamily = "Courier New";
    chart.fontWeight = "500";
    var series = chart.series.push(new am4plugins_wordCloud.WordCloudSeries());
    series.randomness = 0.1;
    series.rotationThreshold = 0.5;

    series.data = [];
    for (let i = 0; i < tags.length; i += 2) {
        series.data.push({
            "t": tags[i],
            "w": tags[i + 1]
        })
    }
    series.minFontSize = 15;
    series.maxFontSize = 50;

    series.dataFields.word = "t";
    series.dataFields.value = "w";

    series.heatRules.push({
        "target": series.labels.template,
        "property": "fill",
        "min": am4core.color("#664d00"),
        "max": am4core.color("#ffbf00"),
        "dataField": "value"
    });

    series.labels.template.events.on("hit", function (ev) {
        ev.event.preventDefault();
        searchReviewsByTag("[" + ev.target.dataItem.properties.word + "]");
    });
    series.labels.template.url = "#";
    series.labels.template.tooltipText = "{word}:\n{value} reviews[/]";
}