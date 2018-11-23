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
    
    $('body').on('click', function (e) {
        switch ($(e.target).prop('tagName')) {
            case 'svg':
                var x = e.originalEvent.clientX;
                var y = e.originalEvent.clientY;

                // console.log($(this).position().left);

                $(e.target).append('<circle cx="' + x + '" cy="' + y + '" r="10" />');
                refresh();
                break;
            case 'circle':
                $(e.target).remove();
                refresh();
                break;
        }
    });

    $('#send').on('click', function (e) {
        var coords = [];
        $('circle').each(function (index, el) {
            var element = $(el);

            coords.push({ x: element.attr('cx'), y: element.attr('cy') })
        });

        console.log(coords);
        $.ajax({ type: 'POST', url: '/CreateMap', data: JSON.stringify(coords), contentType: 'application/json'});
    });
});
   
function refresh() {
    $('#mapCreator').html($('#mapCreator').html());
}
