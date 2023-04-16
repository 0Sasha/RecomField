
$.ajax({
    url: "/Home/GetAllTags",
    type: "POST",
    success: function (tags) {
        let t = tags.split(',');
        if (t.length > 5) initTagCloud(t);
    }
});

function initTagCloud(tags) {
    var chart = am4core.create("tagCloud", am4plugins_wordCloud.WordCloud);
    //chart.fontFamily = "Courier New";
    chart.fontWeight = "400";
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
        "min": am4core.color("#9900cc"),
        "max": am4core.color("#ff66ff"),
        "dataField": "value"
    });

    series.labels.template.url = "/Home/Search/{word}";
    series.labels.template.tooltipText = "{word}:\n[bold]{value}[/]";
    //series.labels.template.urlTarget = "_blank";
}