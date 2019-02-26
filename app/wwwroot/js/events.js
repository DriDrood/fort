var Events = {
    'selected': {
        'source': null,
        'target': null
    },
    'select': function (city) {
        var cityId = city.getAttribute('data-city-id');
        // source
        if (Events.selected.source == null) {
            if (city.hasAttribute('data-owned') && city.getAttribute('data-owned') == 'true') {
                Events.selected.source = city;
                Builder.city.highlight(cityId);
            }
            else {
                Utils.notification('warning', 'Toto město není vaše');
            }
        }

        // target
        else {
            // is neighbour
            if (Events.selected.source.getAttribute('data-neighbours').split(',').includes(cityId)) {
                Events.selected.target = city;
                var sourceCity = Events.selected.source;

                var armySent = sourceCity.getAttribute('data-army-sent-' + cityId);
                var maxArmy = sourceCity.getAttribute('data-army') - (-armySent);

                Utils.modal.setValues(armySent, maxArmy);

                Utils.modal.show();
            }
            else {
                Utils.notification('warning', 'Zde není cesta');
            }
        }
    },
    'deselect': function () {
        if (Events.selected.source == null)
            return;

        Builder.city.unhighlight(Events.selected.source.getAttribute('data-city-id'));
        Events.selected.source = null;
        Events.selected.target = null;
    }
}

document.getElementById('body').onclick = function (event) {
    var target = event.target;
    //// alias
    if (target.hasAttribute('data-for-id')) {
        target = document.getElementById('city-' + target.getAttribute('data-for-id'));
    }

    //// city
    if (target.hasAttribute('data-city-id')) {
        if (Events.selected.source != target)
            Events.select(target);
        else
            Events.deselect(target);
    }
    //// buttons
    // play
    else if (target.id == 'play') {
        Utils.buttons.setSpinner(target);
        Comm.send("play");
    }
    // pause
    else if (target.id == 'pause') {
        Utils.buttons.setSpinner(target);
        Comm.send("pause");
    }
    // end timer
    else if (target.id == 'end') {
        Utils.buttons.setSpinner(target);
        Comm.send("end");
    }
    // restart game
    else if (target.id == 'restart') {
        if (confirm("Opravdu chcete restartovat celou hru?"))
        {
            Utils.buttons.setSpinner(target);
            Comm.send("restart");
        }
    }
    // player is ready
    else if (target.id == 'ready') {
        Utils.buttons.setSpinner(target);
        var isActive = target.classList.contains('active');
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
            sourceCityId: Events.selected.source.getAttribute('data-city-id'),
            targetCityId: Events.selected.target.getAttribute('data-city-id'),
            amount: armySize
        });

        Utils.modal.hide();
    }
    // close modal
    else if (target.id == 'modal_shadow' || target.value == 'cancel') {
        Utils.modal.hide();
    }
};

// Login page
// spinner
document.addEventListener('DOMContentLoaded', function () {
    var loginForm = document.getElementById('loginForm');
    if (loginForm != null) {
        loginForm.onsubmit = function () {
            document.getElementById('loginBtn').innerHTML = '<i class="fa fa-spinner fa-spin fa-fw"></i>';
        };
    }
});
