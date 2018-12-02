// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    $('body.mapCreator').on('click', function (e) {
        switch ($(e.target).prop('tagName')) {
            case 'svg':
                var x = e.originalEvent.clientX;
                var y = e.originalEvent.clientY;

                $(e.target).append('<circle cx="' + x + '" cy="' + y + '" r="10" />');
                refresh();

                $.ajax({ type: 'POST', url: location.pathname + '/AddPoint', data: JSON.stringify({ x: x, y: y }), contentType: 'application/json' });
                break;
            case 'circle':
                var circle = $(e.target);
                var x = circle.attr('cx');
                var y = circle.attr('cy');

                circle.remove();
                refresh();

                $.ajax({ type: 'POST', url: location.pathname + '/DeletePoint', data: JSON.stringify({ x: x, y: y }), contentType: 'application/json' });
                break;
        }
    });

    var sourceCity = null;
    $('body.mapCreator_paths').on('click', function (e) {
        var clicked = $(e.target);
        var clickedX = clicked.attr('cx');
        var clickedY = clicked.attr('cy');
        // click on circle
        if (clicked.prop('tagName') == 'circle') {
            // source
            if (sourceCity == null) {
                sourceCity = { x: clickedX, y: clickedY };
            }

            // target
            else {
                if (sourceCity["x"] == clickedX && sourceCity["y"] == clickedY)
                    return;

                // send
                $.ajax({ type: 'POST', url: location.pathname, data: JSON.stringify({ source: sourceCity, target: { x: clickedX, y: clickedY } }), contentType: 'application/json' });

                // show
                $('#mapCreator').append('<line x1="' + sourceCity["x"] + '" y1="' + sourceCity["y"] + '" x2="' + clickedX + '" y2="' + clickedY + '" style="stroke:black;stroke-width:5;" />');
                refresh();

                // reset source
                sourceCity = null;
            }
        }
    });

    var ws = null;
    var sourceCity = null;
    if ($('body.map').length > 0) {
        var url = 'ws://' + location.hostname + (location.port != '' ? (':' + location.port) : '') + location.pathname;
        console.log('connecting to ' + url);
        ws = new WebSocket(url);
        ws.onmessage = function (message) {
            console.log(message);
            var data = JSON.parse(message.data);
            switch (data["method"]) {
                case "StartRound":
                    var minutes = Math.floor(data["params"] / 60);
                    var seconds = data["params"] % 60;
                    $('#time').html(minutes + ':' + seconds);
                    break;
            }
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
        });
    }
});

function refresh() {
    $('#mapCreator').html($('#mapCreator').html());
}
