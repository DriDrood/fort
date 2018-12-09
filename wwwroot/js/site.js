// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    var sourceCity = null;
    var targetCity = null;
    if ($('body.map').length > 0) {
        createWS();
    }

    $('body.map').on('mousedown', function (e) {
        var clicked = $(e.target);

        // click on circle
        if (clicked.prop('tagName') == 'circle') {
            // source
            if (sourceCity == null) {
                if (clicked.attr('data-owned') != 'True')
                    notification('warning', 'Toto město není vaše');
                else {
                    sourceCity = clicked;
                    clicked.attr('filter', 'url(#shadow)');
                    refresh();
                }
            }

            // target
            else {
                // target = source
                if (sourceCity.attr('data-city-id') == clicked.attr('data-city-id')) {
                    console.log('a');
                    sourceCity.attr('filter', '');
                    sourceCity = null;
                    refresh();
                    return;
                }

                // has no path there
                if (!sourceCity.attr('data-neighbours').split(' ').includes(clicked.attr('data-city-id')))
                    return;

                // save target city
                targetCity = clicked;

                // show modal
                var modal = $('#sendArmy');
                modal.parent().css('display', 'flex');

                // get current army
                var army = sourceCity.attr('data-army-sent-' + targetCity.attr('data-city-id'));
                if (army === undefined || army == null)
                    army = 0;

                // set max army
                modal.find('#armySize')
                    .attr('max', sourceCity.attr('data-army') - (-army))
                    .val(army)
                    .trigger('change', null);
            }
        }
        // send army
        else if (clicked.val() == 'sendArmy') {
            var armySize = $('#armySize').val();
            ws.send(JSON.stringify({
                method: "turn",
                params: {
                    SourceCityId: sourceCity.attr('data-city-id'),
                    TargetCityId: targetCity.attr('data-city-id'),
                    amount: armySize
                }
            }));

            // update army send
            var originalArmy = sourceCity.attr('data-army-sent-' + targetCity.attr('data-city-id'));
            if (originalArmy === undefined || originalArmy == null)
                originalArmy = 0;

            sourceCity.attr('data-army-sent-' + targetCity.attr('data-city-id'), armySize);
            sourceCity.attr('data-army', sourceCity.attr('data-army') - (-originalArmy) - armySize);

            // show shadow
            $('[data-source-id="' + sourceCity.attr('data-city-id') + '"][data-target-id="' + targetCity.attr('data-city-id') + '"],[data-source-id="' + targetCity.attr('data-city-id') + '"][data-target-id="' + sourceCity.attr('data-city-id') + '"]').attr('filter', armySize == 0 ? '' : 'url(#shadow)');
            refresh();

            $('.shadow').css('display', 'none');
            sourceCity.attr('filter', '');
            sourceCity = null;
            targetCity = null;
        }
        // hide modal
        else if (clicked.hasClass('shadow') || clicked.val() == 'cancel') {
            $('.shadow').css('display', 'none');
            sourceCity.attr('filter', '');
            sourceCity = null;
            targetCity = null;
        }
        // actions
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
        // reconect
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

var map_walkOut = null;
var map_walkIn = null;
var map_turns = null;
function onMessage(message) {
    console.log(message);
    var data = JSON.parse(message.data);
    switch (data["method"]) {
        case "notification":
            notification(data['params']['type'], data['params']['message']);
            break;
        case "StartRound":
        case "Resume":
            stopCountDown();
            countDown(data["params"]["duration"]);
            $('#actions .round').html('<i class="fa fa-play" title="Kolo běží"></i> kolo ' + data["params"]["roundNumber"]);
            break;
        case "Pause":
            stopCountDown();
            $('#actions .round').html('<i class="fa fa-pause" title="Kolo je pozastavené"></i> Kolo ' + data["params"]["roundNumber"]);
            break;
        case "EndRound":
            stopCountDown();
            countDown(data["params"]["duration"]);
            $('#actions .round').html('<i class="fa fa-flag-checkered" title="Kolo skončilo"></i> Kolo ' + data["params"]["roundNumber"]);
            break;
        case "InitRound":
            stopCountDown();
            countDown(data["params"]["duration"]);
            $('#actions .round').html('<i class="fa fa-circle-o-notch" title="Začíná kolo"></i> Kolo ' + data["params"]["roundNumber"]);
            break;

        case "map_walkOut":
            map_walkOut = data["params"];
            break
        case "map_walkIn":
            map_walkIn = data["params"];
            break
        case "turns":
            map_turns = data["params"];
            break;
        case "map_show":
            $('#map_holder').html(map_walkIn);
        // TODO
    }
}
function refresh() {
    $('#map_holder').html($('#map_holder').html());
}

function notification(type, message) {
    $('#notification')
        .css('display', 'block')
        .addClass(type)
        .html(message);
    setTimeout(function () {
        document.getElementById('notification').className = '';
        $('#notification').css('display', 'none');
    }, 3000);
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

