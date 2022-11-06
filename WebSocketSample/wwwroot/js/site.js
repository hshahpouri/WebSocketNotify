/** @type{WebSocket} */
var ws = null;

function socket_connect() {

    ws = new WebSocket(`wss://${location.hostname}:${location.port}/ws`);

    ws.onopen = function (ev) {
        console.log("onopen", ev);

        $('#bConnect').addClass("d-none");
        $('#bClose').removeClass("d-none");
        $('#bSend').prop("disabled", false);

        if (Notification.permission == "granted") {
            navigator.serviceWorker.getRegistration().then((reg) => {
                reg.showNotification("WebSocket Connected");
            });
        }
    };

    ws.onerror = function (ev) {
        console.log("onerror", ev);

        if (Notification.permission == "granted") {
            navigator.serviceWorker.getRegistration().then((reg) => {
                reg.showNotification("WebSocket Error, See Console");
            });
        }
    };

    ws.onclose = function (ev) {
        console.log("onclose", ev);

        $('#bConnect').removeClass("d-none");
        $('#bClose').addClass("d-none");
        $('#bSend').prop("disabled", true);

        if (Notification.permission == "granted") {
            navigator.serviceWorker.getRegistration().then((reg) => {
                reg.showNotification("WebSocket Closed");
            });
        }
    };

    ws.onmessage = function (ev) {
        console.log("onmessage", ev);
        $('#divResult>ol').append(`<li>${ev.data}</li>`);

        if (Notification.permission == "granted") {
            navigator.serviceWorker.getRegistration().then((reg) => {
                var options = {
                    body: ev.data,
                    icon: '/WebSocketNotify.png',
                    vibrate: [100, 50, 100, 50, 50],
                    data: {
                        dateOfArrival: Date.now(),
                        primaryKey: 1
                    }
                };
                reg.showNotification("WebSocket New Message", options);
            });
        }
    };
}

function socket_close() {
    ws.close();
}

function socket_send() {
    ws.send($('#tbInput').val());
    $('#tbInput').val("").focus();

    if (Notification.permission == "granted") {
        navigator.serviceWorker.getRegistration().then((reg) => {
            reg.showNotification("Message Sent Over WebSocket");
        });
    }
}

$(function () {


    if ("serviceWorker" in navigator) {
        navigator.serviceWorker.register("/sw.js");
    }

    if (Notification.permission == "granted") {
        $('#divAlert').addClass("alert-success border-success").removeClass("alert-info border-info");
        $('#spanNotificationStatus').addClass("badge-success").removeClass("d-none").html("Notification Allowed");

    }
    else
        if (Notification.permission == "denied") {
            $('#divAlert').addClass("alert-warning border-warning").removeClass("alert-info border-info");
            $('#spanNotificationStatus').addClass("badge-warning").removeClass("d-none").html("Notification Denied");

        }
        else {
            Notification.requestPermission().then((status) => {

                if (status == "granted") {
                    $('#divAlert').addClass("alert-success border-success").removeClass("alert-info border-info");
                    $('#spanNotificationStatus').addClass("badge-success").removeClass("d-none").html("Notification Allowed");
                } else
                    if (status == "denied") {
                        $('#divAlert').addClass("alert-warning border-warning").removeClass("alert-info border-info");
                        $('#spanNotificationStatus').addClass("badge-warning").removeClass("d-none").html("Notification Denied");
                    }
            });
        }

});