var Utils = {
    'middle': function (x1, x2) {
        return (x2 - x1) / 2 + x1;
    },
    'notification': function (type, message) {
        $('#notification')
            .css('display', 'block')
            .addClass(type)
            .html(message);
        setTimeout(function () {
            document.getElementById('notification').className = '';
            $('#notification').css('display', 'none');
        }, 3000);
    },
    'countDown': {
        'task': null,
        'start': function (endAt) {
            countDownTask = setInterval(function () {
                var now = Date.now()
                if (endAt <= now) {
                    stopCountDown();
                    Utils.countDown.setTime(0);
                    return;
                }

                Utils.countDown.setTime(Math.floor((endAt - now) / 1000));
            }, 100);
        },
        'stop': function () {
            clearInterval(Utils.countDown.task);
        },
        'setTime': function (total_seconds) {
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
    },
    'marching': {
        'data': new Audio('/audio/soldiers-marching.mp3'),
        'play': function () {
            Utils.marching.audio.play();
        }
    },
    'modal': {
        'show': function () {
            $('#sendArmy').parent().css('display', 'block').css('display', 'flex');
        },
        'hide': function () {
            $('.shadow .fa-spinner').css('display', 'none');
            $('#sendArmy').css('display', '');
            $('.shadow').css('display', 'none');
            Events.selected.source.attr('filter', '');
            Events.selected.source = null;
            Events.selected.target = null;
        }
    }
}

$(document).ready(function () {
    window.onerror = function (errorMsg, url, lineNumber) {
        Comm.send("jsError", {
            message: errorMsg,
            url: url,
            line: lineNumber
        });
    }

    Utils.notification('success', 'Jste přihlášeni jako ' + $('body').attr('data-player-name'));

    // round is running
    var timerEnd = $('#timerEndsAt');
    if (timerEnd.length > 0) {
        Utils.countDown.start(timerEnd.val());
    }
});
