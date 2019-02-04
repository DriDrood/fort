var ws = null;

var Comm = {
    'connection': null,
    'status': {
        'isConnected': false,
        'connected': function () {
            Comm.status.isConnected = true;
            $('#actions .connection').html('<i class="fa fa-globe" title="Připojen k serveru"></i>');
        },
        'disconnected': function () {
            Comm.status.isConnected = false;
            $('#actions .connection').html('<i class="fa fa-chain-broken" title="Spojení se servererm přerušeno"></i>');
        },
        'connecting': function () {
            Comm.status.isConnected = false;
            $('#actions .connection').html('<i class="fa fa-spinner fa-spin fa-fw" title="Navazuji spojení..."></i>');
        }
    },
    'createConnection': function () {
        var url = 'ws://' + location.hostname + (location.port != '' ? (':' + location.port) : '') + location.pathname;
        console.log('connecting to ' + url);
        ws = new WebSocket(url);
        ws.onopen = function () {
            Comm.status.connected();
        };
        ws.onmessage = function (message) {
            console.log('Recieved message: ' + message);
            var messageJson = JSON.parse(message.data);
            Comm.onMessage(messageJson['method'], messageJson['param']);
        };
        ws.onclose = function () {
            Comm.status.disconnected();
        };
    },
    'send': function (method, data) {
        if (!Comm.status.isConnected) {
            Utils.notification('warning', 'Nejste připojen');
            return;
        }

        if (data === undefined || data == null)
            data = {};

        var dataString = JSON.stringify({ method: method, param: data });
        ws.send(dataString);
    },
    'onMessage': function (method, data) {
        switch (method) {

        }
    }
}

$(document).ready(function () {
    if ($('body.map').length > 0) {
        Comm.createConnection();
    }
});