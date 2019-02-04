var Events = {
    'selected': {
        'source': null,
        'target': null
    },
    'select': function (city) {
        // source
        if (Events.selected.source == null) {
            if (city.hasAttribute('data-owned') && city.getAttribute('data-owned')) {
                Events.selected.source = city;
                Builder.city.highlight(city.getAttribute('data-city-id'));
            }
            else {
                Utils.notification('warning', 'Toto město není vaše');
            }
        }

        // target
        else {

        }
    }
}

$('body').on('click', function (event) {
    var target = event.target;
    //// alias
    if (target.hasAttribute('data-for-id')) {
        target = document.getElementById(target.getAttribute('data-for-id'));
    }

    //// city
    if (target.hasAttribute('data-city-id')) {
        Events.select(target);
    }
    //// buttons
    // play
    else if (target.id == 'play') {
        Comm.send("play");
    }
    // pause
    else if (target.id == 'pause') {
        Comm.send("pause");
    }
    // end timer
    else if (target.id == 'end') {
        Comm.send("end");
    }
    // restart game
    else if (target.id == 'restart') {
        if (confirm("Opravdu chcete restartovat celou hru?"))
            Comm.send("restart");
    }
    // player is ready
    else if (target.id == 'ready') {
        var isActive = target.classList.contains('active');
        if (isActive)
            target.classList.remove('active');
        else
            target.classList.add('active');

        Comm.send("playerReady", { ready: !isActive });
    }
    //// reconnect
    else if (target.classList.contains('fa-chain-broken')) {
        Comm.status.connecting();
        Comm.createConnection();
    }
    //// modal
    // ok button
    else if (target.value == 'sendArmy') {
        var armySize = document.getElementById('armySize').value;
        Comm.send("turn", {
            SourceCityId: Events.selected.source.attr('data-city-id'),
            TargetCityId: Events.selected.target.attr('data-city-id'),
            amount: armySize
        });

        $('#sendArmy').css('display', 'none');
        $('.shadow .fa-spinner').css('display', 'inline');
    }
    // close modal
    else if (target.classList.contains('shadow') || target.value == 'cancel') {
        Utils.modal.hide();
    }
});

// Login page
// spinner
$('body.login form').on('submit', function () {
    $('#loginBtn').html('<i class="fa fa-spinner fa-spin fa-fw"></i>');
});
