/** @type{WebSocket} */
var ws = null;

function socket_connect() {

    ws = new WebSocket("wss://localhost:5001/wsn");
    
    ws.onopen = function (ev) {
        console.log("onopen", ev);

        $('#bConnect').addClass("d-none");
        $('#bClose').removeClass("d-none");
    };

    ws.onerror = function (ev) {
        console.log("onerror", ev);

    };

    ws.onclose = function (ev) {
        console.log("onclose", ev);

        $('#bConnect').removeClass("d-none");
        $('#bClose').addClass("d-none");
    };

    ws.onmessage = function (ev) {
        console.log("onmessage", ev);
        $('#divResult>ol').append(`<li>${ev.data}</li>`);
    };
}

function socket_close() {
    ws.close();
}

function socket_send() {
    ws.send($('#tbInput').val());
}