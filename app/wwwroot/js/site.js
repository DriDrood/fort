// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// DATA
var sourceCity = null;
var targetCity = null;
var ws = null;
var httpDone = true;
var countDownTask;
var animationDuration = 3000;
var audioMarching = new Audio('/audio/soldiers-marching.mp3');

$(document).ready(function () {
    notification('success', 'Jste přihlášeni jako ' + $('body').attr('data-player-name'));
    if ($('body.map').length > 0) {
        CreateConnection();

        // round is running
        if ($('.round .fa-play').length > 0) {
            var time = $('.time').html().split(':');
            countDown(time[0] * 60 * 60 - (-time[1] * 60) - (-time[2]));
        }
    }

    // Click handling
    $('body.map').on('mousedown', function (e) {
        var clicked = $(e.target);

        // click on circle
        if (clicked.prop('tagName') == 'circle') {
            if (clicked.attr('data-for') !== undefined || clicked.attr('data-for') != null)
                clicked = $('circle[data-city-id=' + clicked.attr('data-for') + ']');

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
                    sourceCity.attr('filter', '');
                    sourceCity = null;
                    refresh();
                    return;
                }

                // has no path there
                var included = false;
                $.each(sourceCity.attr('data-neighbours').split(' '), function (index, element) {
                    if (element == clicked.attr('data-city-id'))
                        included = true;
                });
                if (!included)
                    return;

                // save target city
                targetCity = clicked;

                // show modal
                var modal = $('#sendArmy');
                modal.parent().css('display', 'block').css('display', 'flex');

                // get current army
                var army = sourceCity.attr('data-army-sent-' + targetCity.attr('data-city-id'));
                if (army === undefined || army == null)
                    army = 0;

                var max = sourceCity.attr('data-army') - (-army);

                // set max army
                modal.find('#armySize')
                    .attr('max', max)
                    .val(army == 0 ? max : army)
                    .trigger('change', null);
            }
        }
        // send army
        else if (clicked.val() == 'sendArmy') {
            var armySize = $('#armySize').val();
            Send("turn", {
                SourceCityId: sourceCity.attr('data-city-id'),
                TargetCityId: targetCity.attr('data-city-id'),
                amount: armySize
            });

            $('#sendArmy').css('display', 'none');
            $('.shadow .fa-spinner').css('display', 'inline');
        }
        // hide modal
        else if (clicked.hasClass('shadow') || clicked.val() == 'cancel') {
            hideModal();
        }
        // actions
        else if (clicked.attr('id') == 'play') {
            Send("play");
        }
        else if (clicked.attr('id') == 'pause') {
            Send("pause");
        }
        else if (clicked.attr('id') == 'end') {
            Send("end");
        }
        else if (clicked.attr('id') == 'restart') {
            if (confirm("Opravdu chcete restartovat celou hru?"))
                Send("restart");
        }
        else if (clicked.attr('id') == 'ready') {
            clicked.toggleClass('active');
            Send("playerReady", { ready: clicked.hasClass('active') });
        }
        // reconect
        else if (clicked.hasClass('fa-chain-broken')) {
            clicked.parent().html('<i class="fa fa-spinner fa-spin fa-fw"></i>')
            CreateConnection();
        }
    });
});



// Message Handling
var map_walkOut = null;
var map_walkIn = null;
var map_turns = null;
function onMessage(data) {
    switch (data["method"]) {
        // notifications
        case "notification":
            notification(data['param']['type'], data['param']['message']);
            break;

        // round lifecycle
        case "StartRound":
        case "Resume":
            stopCountDown();
            countDown(data["param"]["duration"]);
            $('#actions .round').html('<i class="fa fa-play" title="Kolo běží"></i> kolo ' + data["param"]["roundNumber"]);
            break;
        case "Pause":
            stopCountDown();
            $('#actions .round').html('<i class="fa fa-pause" title="Kolo je pozastavené"></i> Kolo ' + data["param"]["roundNumber"]);
            break;
        case "EndRound":
            stopCountDown();
            countDown(data["param"]["duration"]);
            $('#actions .round').html('<i class="fa fa-flag-checkered" title="Kolo skončilo"></i> Kolo ' + data["param"]["roundNumber"]);
            break;
        case "InitRound":
            stopCountDown();
            countDown(data["param"]["duration"]);
            $('#actions .round').html('<i class="fa fa-circle-o-notch" title="Začíná kolo"></i> Kolo ' + data["param"]["roundNumber"]);
            break;
        case "Restart":
            stopCountDown();
            setTime(0);
            $('#map_holder').html(data["param"]);
            break;

        // round results
        case "map_walkOut":
            map_walkOut = data["param"];
            break
        case "map_walkIn":
            map_walkIn = data["param"];
            break
        case "turns":
            map_turns = JSON.parse(data["param"]);
            break;
        case "map_show":
            var mapHolder = $('#map_holder');
            mapHolder.html(map_walkOut);
            $('#ready').removeClass('active');
            // if ($('body.Team').length > 0)
            audioMarching.play();

            var ratio = parseInt($('#map').css('width')) / parseInt($('#map').attr('viewBox').split(' ')[2]);
            $.each(map_turns, function (index, turn) {
                var turnObject = $(turn);
                turnObject.css('top', marginFromCenter(turnObject.css('top'), $('#map').css('margin-top')));
                turnObject.css('left', marginFromCenter(turnObject.css('left'), 0));

                var duration = turnObject.attr('data-time') == 'all' ? animationDuration : (animationDuration / 2);
                var delay = turnObject.attr('data-time') == 'end' ? (animationDuration / 2) : 0;
                setTimeout(function () {
                    mapHolder.append(turnObject[0].outerHTML);
                    $('#' + turnObject.attr('id')).animate({ left: marginFromCenter(turnObject.attr('data-final-x'), 0), top: marginFromCenter(turnObject.attr('data-final-y'), $('#map').css('margin-top')) }, duration, function () {
                        $('#' + turnObject.attr('id')).remove();
                    });
                }, delay);

                function marginFromCenter(center, margin) {
                    return (parseInt(center) * ratio) - (parseInt(turnObject.css('width')) / 2) - (-parseInt(margin)) + 'px';
                }
            });

            setTimeout(function () {
                mapHolder.html(map_walkIn);
                audioMarching.pause();
                audioMarching.currentTime = 0;
            }, animationDuration);
            break;

        // turn response
        case "turnOk":
            var armySize = $('#armySize').val();
            // update army send
            var originalArmy = sourceCity.attr('data-army-sent-' + targetCity.attr('data-city-id'));
            if (originalArmy === undefined || originalArmy == null)
                originalArmy = 0;

            sourceCity.attr('data-army-sent-' + targetCity.attr('data-city-id'), armySize);
            sourceCity.attr('data-army', sourceCity.attr('data-army') - (-originalArmy) - armySize);

            // show shadow
            $('[data-source-id="' + sourceCity.attr('data-city-id') + '"][data-target-id="' + targetCity.attr('data-city-id') + '"],[data-source-id="' + targetCity.attr('data-city-id') + '"][data-target-id="' + sourceCity.attr('data-city-id') + '"]').attr('filter', armySize == 0 ? '' : 'url(#shadow)');
            refresh();

            hideModal();
            notification('success', data['param']);
            break;
        case "turnError":
            hideModal();
            notification('error', data['param']);
            break;

        // admin stuff
        case "playerConnected":
            if ($('.statistics').length > 0) {
                $('[data-playerId="' + data['param']['playerId'] + '"] .fa-chain-broken').removeClass('fa-chain-broken').addClass('fa-globe');
            }
            break;
        case "playerDisconnected":
            if ($('.statistics').length > 0) {
                $('[data-playerId="' + data['param']['playerId'] + '"] .fa-globe').removeClass('fa-globe').addClass('fa-chain-broken');
            }
            break;
        case "playerReady":
            if ($('.statistics').length > 0) {
                if (data["param"]["ready"]) {
                    $('[data-playerId="' + data['param']['playerId'] + '"]').append('<i class="fa fa-check"></i>');
                }
                else {
                    $('[data-playerId="' + data['param']['playerId'] + '"] .fa-check').remove();
                }
            }
            break;
        case "statistics":
            $('.statistics').remove();
            $('body').append(data["param"]);
            break;
    }
}

// Connection
function CreateConnection() {
    // WebSocket
    if (window.WebSocket) {
        var url = 'ws://' + location.hostname + (location.port != '' ? (':' + location.port) : '') + location.pathname;
        console.log('connecting to ' + url);
        ws = new WebSocket(url);
        ws.onopen = function () {
            $('#actions .connection').html('<i class="fa fa-globe" title="Připojen k serveru"></i>');
        };
        ws.onmessage = function (message) {
            onMessage(JSON.parse(message.data));
        };
        ws.onclose = function () {
            $('#actions .connection').html('<i class="fa fa-chain-broken" title="Spojení se servererm přerušeno"></i>');
        };
    }
    // Http
    else {
        var baseUrl = 'http://' + location.hostname + (location.port != '' ? (':' + location.port) : '') + location.pathname;
        $.ajax({
            url: baseUrl + '/Connect',
            success: function (data) {
                if (data == 'Done') {
                    $('#actions .connection').html('<i class="fa fa-globe" title="Připojen k serveru"></i>');
                }
            }
        });
        setInterval(function () {
            if (!httpDone)
                return;

            httpDone = false;
            $.ajax({
                url: baseUrl + '/GetQueue',
                success: function (data) {
                    console.log(data);
                    var jsonData = JSON.parse(data);
                    $.each(jsonData, function (index, message) {
                        onMessage(message);
                    });
                },
                error: function () {
                    $('#actions .connection').html('<i class="fa fa-chain-broken" title="Spojení se servererm přerušeno"></i>');
                },
                complete: function () {
                    httpDone = true;
                }
            })
        }, 2000);
    }
}
function Send(method, message) {
    if (message === undefined || message == null)
        message = {};

    var data = JSON.stringify({ method: method, param: message });
    if (window.WebSocket) {
        ws.send(data);
    }
    else {
        var baseUrl = 'http://' + location.hostname + (location.port != '' ? (':' + location.port) : '') + location.pathname;
        $.ajax({
            url: baseUrl + '/PostMessage',
            method: 'POST',
            contentType: 'application/json',
            data: data
        });
    }
}

// Notifications
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

// CountDown
function countDown(total_seconds) {
    countDownTask = setInterval(function () {
        setTime(total_seconds);

        total_seconds--;
        if (total_seconds < 0)
            stopCountDown();
    }, 1000);
}
function stopCountDown() {
    clearInterval(countDownTask);
}
function setTime(total_seconds) {
    var hours = Math.floor(total_seconds / (60 * 60));
    var minutes = Math.floor(total_seconds / 60) % 60;
    var seconds = total_seconds % 60;
    $('#actions .time').html(hours + ':' + pad100(minutes) + ':' + pad100(seconds));

    function pad100(value) {
        if (value < 10)
            return '0' + value;

        return value;
    }
}

// System
function refresh() {
    $('#map_holder').html($('#map_holder').html());
}
function hideModal() {
    $('.shadow .fa-spinner').css('display', 'none');
    $('#sendArmy').css('display', '');
    $('.shadow').css('display', 'none');
    sourceCity.attr('filter', '');
    sourceCity = null;
    targetCity = null;
}
window.onerror = function (errorMsg, url, lineNumber) {
    Send("jsError", {
        message: errorMsg,
        url: url,
        line: lineNumber
    });
}

// LoginPage
$(document).ready(function () {
    // spinner
    $('body.login form').on('submit', function () {
        $('#loginBtn').html('<i class="fa fa-spinner fa-spin fa-fw"></i>');
    });
});
