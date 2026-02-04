"use strict";

const connection = new signalR.HubConnectionBuilder()    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", function (title, message) {
    showNativeNotification(title, message);
});

connection.start()
    .then(function () {
        console.log("SignalR connected.");
        requestPermission();
    })
    .catch(function (err) {
        return console.error(err.toString());
    });

function requestPermission() {
    if (!("Notification" in window)) {
        return;
    }

    if (Notification.permission !== "granted" && Notification.permission !== "denied") {
        Notification.requestPermission();
    }
}

function showNativeNotification(title, bodyText) {
    if (Notification.permission === "granted") {
        const notification = new Notification(title, {
            body: bodyText,
            icon: '/img/car-icon.png',
            vibrate: [200, 100, 200]
        });

        notification.onclick = function () {
            window.focus();
            this.close();
            window.location.href = '/Requests/MyRequests';
        };
    }
}