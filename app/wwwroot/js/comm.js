var ws = null;

var Comm = {
    'connection': null,
    'status': {
        'active': 'disconnected',
        'connected': function () {
            Comm.status.active = 'connected';
            document.getElementById('connection').innerHTML = '<i class="fa fa-globe" title="Připojen k serveru"></i>';
        },
        'disconnected': function () {
            Comm.status.active = 'disconnected';
            document.getElementById('connection').innerHTML = '<i class="fa fa-chain-broken" title="Spojení se servererm přerušeno"></i>';
        },
        'connecting': function () {
            Comm.status.active = 'connecting';
            document.getElementById('connection').innerHTML = '<i class="fa fa-spinner fa-spin fa-fw" title="Navazuji spojení..."></i>';
        }
    },
    'createConnection': function () {
        Comm.status.connecting();
        var url = 'ws://' + location.hostname + (location.port != '' ? (':' + location.port) : '') + location.pathname;
        console.log('connecting to ' + url);
        ws = new WebSocket(url);
        ws.onopen = function () {
            Comm.status.connected();
        };
        ws.onmessage = function (message) {
            console.log('Recieved message: ', message);
            var messageJson = JSON.parse(message.data);
            Comm.onMessage(messageJson['method'], messageJson['param']);
        };
        ws.onclose = function () {
            Comm.status.disconnected();
        };
    },
    'send': function (method, data) {
        if (Comm.status.active != 'connected') {
            if (Comm.status.active == 'disconnected') {
                Comm.createConnection();
            }
            else {
                Utils.notification('warning', 'Nejste připojen');
                return;
            }
        }

        if (data === undefined || data == null)
            data = {};

        var dataString = JSON.stringify({ method: method, param: data });
        ws.send(dataString);
    },
    'onMessage': function (method, data) {
        switch (method) {
            // Notification
            case 'notification':
                Utils.notification(data['type'], data['message']);
                break;
            // Round handling
            case "StartRound":
            case "Resume":
                Utils.countDown.start(Date.parse(data['endsAt']));
                Utils.roundStatus.running(data['roundNumber'])
                Utils.buttons.removeSpinner(document.getElementById('play'));
                break;
            case "Pause":
                Utils.countDown.stop();
                Utils.roundStatus.paused(data['roundNumber'])
                Utils.buttons.removeSpinner(document.getElementById('pause'));
                break;
            case "EndRound":
                Utils.countDown.stop();
                Utils.roundStatus.ended(data['roundNumber'])
                Utils.countDown.start(Date.parse(data['endsAt']));
                Utils.buttons.removeSpinner(document.getElementById('end'));
                break;
            case "InitRound":
                Utils.countDown.stop();
                Utils.roundStatus.new(data['roundNumber'])
                Utils.countDown.start(Date.parse(data['endsAt']));
                break;
            case "Restart":
                Utils.countDown.stop();
                Utils.countDown.setTime(0);

                Builder.clean();
                Builder.INIT(data['map']);
                Utils.buttons.removeSpinner(document.getElementById('restart'));
                break;

            case 'playerReady_ok':
                Utils.playerReady.setReady(data['ready']);
                break;
            case 'end_ok':
                Utils.buttons.removeSpinner(document.getElementById('end'));
                break;

            // turn response
            case "turnOk":
                var sourceCity = Events.selected.source;
                var targetCity = Events.selected.target;
                var sourceId = sourceCity.getAttribute('data-city-id');
                var targetId = targetCity.getAttribute('data-city-id');

                // update army send
                var newArmy = document.getElementById('armySize').value;
                var originalArmy = Events.selected.source.getAttribute('data-army-sent-' + targetId);
                if (originalArmy === undefined || originalArmy == null)
                    originalArmy = 0;

                sourceCity.setAttribute('data-army-sent-' + targetId, newArmy);
                sourceCity.setAttribute('data-army', sourceCity.getAttribute('data-army') - (-originalArmy) - newArmy);

                // show turn
                Builder.turn.createUpdate(data['pathId'], data['reversDirection'], newArmy);

                // finalize
                Utils.modal.hide();
                Utils.notification('success', data['message']);
                break;
            case "turnError":
                Utils.modal.hide();
                Utils.notification('error', data['message']);
                break;

            // round results
            case 'roundResults':
                // TODO

                Utils.marching.play();
                break;

            // statistics
            // case "playerConnected":
            //     if ($('.statistics').length > 0) {
            //         $('[data-playerId="' + data['param']['playerId'] + '"] .fa-chain-broken').removeClass('fa-chain-broken').addClass('fa-globe');
            //     }
            //     break;
            // case "playerDisconnected":
            //     if ($('.statistics').length > 0) {
            //         $('[data-playerId="' + data['param']['playerId'] + '"] .fa-globe').removeClass('fa-globe').addClass('fa-chain-broken');
            //     }
            //     break;
            // case "playerReady":
            //     if ($('.statistics').length > 0) {
            //         if (data["param"]["ready"]) {
            //             $('[data-playerId="' + data['param']['playerId'] + '"]').append('<i class="fa fa-check"></i>');
            //         }
            //         else {
            //             $('[data-playerId="' + data['param']['playerId'] + '"] .fa-check').remove();
            //         }
            //     }
            //     break;
            // case "statistics":
            //     $('.statistics').remove();
            //     $('body').append(data["param"]);
            //     break;
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('body').classList.contains('map')) {
        console.log('connecting');
        Comm.createConnection();
    }
});