var Utils = {
    'middle': function (x1, x2) {
        return (x2 - x1) / 2 - (-x1);
    },
    'notification': function (type, message) {
        var element = document.getElementById('notification');
        element.style.display = 'block';
        element.className = type;
        element.innerHTML = message;

        setTimeout(function () {
            element.className = '';
            element.style.display = 'none';
        }, 3000);
    },
    'countDown': {
        'task': null,
        'start': function (endAt) {
            countDownTask = setInterval(function () {
                var now = Date.now()
                if (endAt <= now) {
                    Utils.countDown.stop();
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
            document.getElementById('time').innerHTML = hours + ':' + pad100(minutes) + ':' + pad100(seconds);

            function pad100(value) {
                if (value < 10)
                    return '0' + value;

                return value;
            }
        }
    },
    'buttons': {
        'setSpinner': function (target) {
            target.classList.add('fa-spin');
            target.classList.add('fa-fw');
        },
        'removeSpinner': function (target) {
            target.classList.remove('fa-spin');
            target.classList.remove('fa-fw');
        }
    },
    'playerReady': {
        'setReady': function (ready) {
            var target = document.getElementById('ready');

            if (ready)
                target.classList.add('active');
            else
                target.classList.remove('active');
            Utils.buttons.removeSpinner(target);
        }
    },
    'roundStatus': {
        'new': function (roundNumber) {
            document.getElementById('round').innerHTML =
                '<i class="fa fa-circle-o-notch" title="Začíná kolo"></i> Kolo ' + roundNumber;
        },
        'running': function (roundNumber) {
            document.getElementById('round').innerHTML =
                '<i class="fa fa-play" title="Kolo běží"></i> kolo ' + roundNumber;
        },
        'paused': function (roundNumber) {
            document.getElementById('round').innerHTML =
                '<i class="fa fa-pause" title="Kolo je pozastavené"></i> Kolo ' + roundNumber;
        },
        'ended': function (roundNumber) {
            document.getElementById('round').innerHTML =
                '<i class="fa fa-flag-checkered" title="Kolo skončilo"></i> Kolo ' + roundNumber;
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
            var shadow = document.getElementById('modal_shadow');
            shadow.style.display = 'block';
            shadow.style.display = 'flex';
        },
        'showSpinner': function () {
            document.getElementById('sendArmy').style.display = 'none';
            document.getElementById('modal_spinner').style.display = 'inline';
        },
        'hide': function () {
            document.getElementById('modal_spinner').style.display = 'none';
            document.getElementById('sendArmy').style.display = '';
            document.getElementById('modal_shadow').style.display = 'none';

            Events.selected.source.setAttribute('filter', '');
            Events.selected.source = null;
            Events.selected.target = null;
        },
        'setValues': function (current, max) {
            var slider = document.getElementById('armySize');

            slider.setAttribute('max', max);

            slider.value = (current == null || current === undefined)
                ? max
                : current;

            // move slider
            var setSlider = new Event('change');
            slider.dispatchEvent(setSlider);
        }
    }
}

window.onerror = function (errorMsg, url, lineNumber) {
    Comm.send("jsError", {
        message: errorMsg,
        url: url,
        line: lineNumber
    });
}
document.addEventListener('DOMContentLoaded', function () {
    Utils.notification('success', 'Jste přihlášeni jako ' + document.getElementById('body').getAttribute('data-player-name'));
});
