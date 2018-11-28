// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    var ws;
    $('body.mapCreator').on('click', function (e) {
        switch ($(e.target).prop('tagName')) {
            case 'svg':
                var x = e.originalEvent.clientX;
                var y = e.originalEvent.clientY;

                $(e.target).append('<circle cx="' + x + '" cy="' + y + '" r="10" />');
                refresh();

                $.ajax({ type: 'POST', url: '/Admin/MapCreator/AddPoint', data: JSON.stringify({ x: x, y: y }), contentType: 'application/json' });
                break;
            case 'circle':
                var circle = $(e.target);
                var x = circle.attr('cx');
                var y = circle.attr('cy');

                circle.remove();
                refresh();

                $.ajax({ type: 'POST', url: '/Admin/MapCreator/DeletePoint', data: JSON.stringify({ x: x, y: y }), contentType: 'application/json' });
                break;
        }
    });

    if ($('body.map').length > 0) {
        var url = 'ws://' + location.hostname + (location.port != '' ? (':' + location.port) : '') + location.pathname;
        console.log('connecting to ' + url);
        ws = new WebSocket(url);
        ws.onmessage = function (message) {
            console.log(message);
            var data = JSON.parse(message.data);
            switch(data["method"])
            {
                case "StartRound":
                    var minutes = Math.floor(data["params"] / 60);
                    var seconds = data["params"] % 60;
                    $('#time').html(minutes + ':' + seconds);
                    break;
            }
        }
    }

    $('#play').on('click', function () {
        var data = { method: "play" }
        ws.send(JSON.stringify(data));
    });
    $('#pause').on('click', function () {
        var data = { method: "pause" }
        ws.send(JSON.stringify(data));
    });
    $('#resume').on('click', function () {
        var data = { method: "resume" }
        ws.send(JSON.stringify(data));
    });
    $('#end').on('click', function () {
        var data = { method: "end" }
        ws.send(JSON.stringify(data));
    });
});

function refresh() {
    $('#mapCreator').html($('#mapCreator').html());
}
