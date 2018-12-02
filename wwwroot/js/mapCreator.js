$(document).ready(function () {
    $('body.mapCreator').on('click', function (e) {
        switch ($(e.target).prop('tagName')) {
            case 'svg':
                var x = e.originalEvent.clientX;
                var y = e.originalEvent.clientY;

                $(e.target).append('<circle cx="' + x + '" cy="' + y + '" r="10" />');
                refresh();

                $.ajax({ type: 'POST', url: location.pathname + '/AddPoint', data: JSON.stringify({ x: x, y: y }), contentType: 'application/json' });
                break;
            case 'circle':
                var circle = $(e.target);
                var x = circle.attr('cx');
                var y = circle.attr('cy');

                circle.remove();
                refresh();

                $.ajax({ type: 'POST', url: location.pathname + '/DeletePoint', data: JSON.stringify({ x: x, y: y }), contentType: 'application/json' });
                break;
        }
    });

    var sourceCity = null;
    $('body.mapCreator_paths').on('click', function (e) {
        var clicked = $(e.target);
        var clickedX = clicked.attr('cx');
        var clickedY = clicked.attr('cy');
        // click on circle
        if (clicked.prop('tagName') == 'circle') {
            // source
            if (sourceCity == null) {
                sourceCity = { x: clickedX, y: clickedY };
            }

            // target
            else {
                if (sourceCity["x"] == clickedX && sourceCity["y"] == clickedY)
                    return;

                // send
                $.ajax({ type: 'POST', url: location.pathname, data: JSON.stringify({ source: sourceCity, target: { x: clickedX, y: clickedY } }), contentType: 'application/json' });

                // show
                $('#mapCreator').append('<line x1="' + sourceCity["x"] + '" y1="' + sourceCity["y"] + '" x2="' + clickedX + '" y2="' + clickedY + '" style="stroke:black;stroke-width:5;" />');
                refresh();

                // reset source
                sourceCity = null;
            }
        }
    });
});