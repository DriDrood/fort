// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    $('#turn').on('click', function () {
        var url = window.location.protocol + "//" + window.location.hostname;
        if (window.location.port != 80)
            url += ":" + window.location.port;

        $.get(url + "/turn", function (data) {
            $.each(data.round, function (index, turn) {
                console.log(turn.element);
                $('body').append(turn.element);
                $('#army' + turn.id).animate({ left: turn.finalx, top: turn.finaly }, 1000, function () {
                    $('#map').html(data.map);
                    $('.army').remove();
                });
            });
        });
    });

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
});

function refresh() {
    $('#mapCreator').html($('#mapCreator').html());
}
