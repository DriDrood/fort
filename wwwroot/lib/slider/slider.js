$(document).ready(function () {
    var slider = null;
    $('.slider .slider_point').on('mousedown', function (e) {
        var sliderPoint = $(this);
        slider = { object: sliderPoint, x: e.originalEvent.clientX, margin: parseInt(sliderPoint.css('left')) };
    });

    $('body').on('mousemove', function (e) {
        if (slider != null) {
            var parentWidth = parseInt(slider.object.parent().css('width'));
            var xDiff = e.originalEvent.clientX - slider['x'];
            var currentMargin = slider['margin'] + xDiff;

            // is in range
            if (currentMargin > -5 && currentMargin < (parentWidth - 5)) {
                // move
                slider['object'].css('left', currentMargin);

                // set output
                var target = $('#' + slider['object'].parent().attr('data-target'));
                var fieldSize = parseInt(slider['object'].parent().css('width')) / (target.attr('max') - target.attr('min'));

                target.val(Math.floor((currentMargin + fieldSize / 2) / fieldSize));
            }
        }
    });

    $('[data-source]').on('keyup keypress blur change click', function () {
        var thisObject = $(this);
        var source = $('#' + thisObject.attr('data-source'));

        var value = parseInt(thisObject.val());
        var max = parseInt(thisObject.attr('max'));
        var min = parseInt(thisObject.attr('min'));
        if (thisObject.val() != '' && value <= max && value >= min) {
            var field = parseInt(source.css('width')) / (max - min);
            var point = source.find('.slider_point');
            point.css('left', (field * value) - 5);
        }
    });

    $('body').on('mouseup', function (e) {
        if (slider != null)
            slider = null;
    });
});
