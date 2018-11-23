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
});
