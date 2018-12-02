// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    var sourceCity = null;
    if ($('body.map').length > 0) {
        createWS();
    }

    $('body.map').on('click', function (e) {
        var clicked = $(e.target);

        // click on circle
        if (clicked.prop('tagName') == 'circle') {
            // source
            if (sourceCity == null) {
                sourceCity = { x: clicked.attr('cx'), y: clicked.attr('cy') };
            }

            // target
            else {
                ws.send(JSON.stringify({
                    method: "turn",
                    params: {
                        source: {
                            x: sourceCity.x,
                            y: sourceCity.y
                        },
                        target: {
                            x: clicked.attr('cx'),
                            y: clicked.attr('cy')
                        }
                    }
                }));
                sourceCity = null;
            }
        }
        else if (clicked.attr('id') == 'play') {
            ws.send(JSON.stringify({ method: "play" }));
        }
        else if (clicked.attr('id') == 'pause') {
            ws.send(JSON.stringify({ method: "pause" }));
        }
        else if (clicked.attr('id') == 'resume') {
            ws.send(JSON.stringify({ method: "resume" }));
        }
        else if (clicked.attr('id') == 'end') {
            ws.send(JSON.stringify({ method: "end" }));
        }
        else if (clicked.hasClass('fa-chain-broken')) {
            clicked.parent().html('<i class="fa fa-spinner fa-spin fa-fw"></i>')
            createWS();
        }
    });
});

var ws = null;
function createWS() {
    var url = 'ws://' + location.hostname + (location.port != '' ? (':' + location.port) : '') + location.pathname;
    console.log('connecting to ' + url);
    ws = new WebSocket(url);
    ws.onopen = function () {
        $('#actions .connection').html('<i class="fa fa-globe" title="Připojen k serveru"></i>');
    };
    ws.onmessage = function (message) {
        onMessage(message);
    };
    ws.onclose = function () {
        $('#actions .connection').html('<i class="fa fa-chain-broken" title="Spojení se servererm přerušeno"></i>');
    };
}

function onMessage(message) {
    console.log(message);
    var data = JSON.parse(message.data);
    switch (data["method"]) {
        case "StartRound":
        case "Resume":
            countDown(data["params"]["duration"]);
            $('#actions .round').html('Běží kolo ' + data["params"]["roundNumber"]);
            break;
        case "Pause":
            stopCountDown();
            $('#actions .round').html('Pozastavené Kolo ' + data["params"]["roundNumber"]);
            break;
        case "EndRound":
            stopCountDown();
            countDown(data["params"]["duration"]);
            $('#actions .round').html('Ukončené Kolo ' + data["params"]["roundNumber"]);
            break;
    }
}

function refresh() {
    $('#mapCreator').html($('#mapCreator').html());
}

var countDownTask;
function countDown(total_seconds) {
    countDownTask = setInterval(function () {
        var hours = Math.floor(total_seconds / (60 * 60));
        var minutes = Math.floor(total_seconds / 60);
        var seconds = total_seconds % 60;
        $('#actions .time').html(hours + ':' + pad100(minutes) + ':' + pad100(seconds));

        total_seconds--;
        if (total_seconds < 0)
            stopCountDown();
    }, 1000);

    function pad100(value) {
        if (value < 10)
            return '0' + value;
    
        return value;
    }
}
function stopCountDown() {
    clearInterval(countDownTask);
}

