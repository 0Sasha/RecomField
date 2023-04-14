"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").withAutomaticReconnect().build();

connection.on("NewReviewComment", function (id) {
    updateReviewComments(id);
});

connection.start().then(function () {
    console.log('Hub is connected');
}).catch(function (err) {
    return console.error(err.toString());
});