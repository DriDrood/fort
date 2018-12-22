$(document).ready(function () {
    var slider = null;
    $('.slider .slider_point').on('mousedown touchstart', function (e) {
        var sliderPoint = $(this);
        slider = { object: sliderPoint, x: e.touches === undefined ? e.originalEvent.clientX : e.touches[0].clientX, margin: parseInt(sliderPoint.css('left')) };
    });

    $('.slider').on('click', function (e) {
        if (this == e.target)
        {
            var sliderObject = $('.slider_point');
            var currentMargin = e.originalEvent.offsetX - 15;
            
            // move
            sliderObject.css('left', currentMargin);

            // set output
            var target = $('#' + sliderObject.parent().attr('data-target'));
            var fieldSize = parseInt(sliderObject.parent().css('width')) / (target.attr('max') - target.attr('min'));

            target.val(Math.floor(((currentMargin + 15) + fieldSize / 2) / fieldSize));
        }
    });

    $('body').on('mousemove touchmove', function (e) {
        if (slider != null) {
            var parentWidth = parseInt(slider.object.parent().css('width'));
            var currentX = e.touches === undefined ? e.originalEvent.clientX : e.touches[0].clientX;
            var xDiff = currentX - slider['x'];
            var currentMargin = slider['margin'] + xDiff;

            // is in range
            if (currentMargin < -15)
                currentMargin = -15;
            else if (currentMargin > (parentWidth - 15))
                currentMargin = parentWidth - 15;

            // move
            slider['object'].css('left', currentMargin);

            // set output
            var target = $('#' + slider['object'].parent().attr('data-target'));
            var fieldSize = parseInt(slider['object'].parent().css('width')) / (target.attr('max') - target.attr('min'));

            target.val(Math.floor(((currentMargin + 15) + fieldSize / 2) / fieldSize));
        }
    });

    $('[data-source]').on('keyup keypress blur change click', function () {
        var thisObject = $(this);
        var source = $('#' + thisObject.attr('data-source'));

        var value = parseInt(thisObject.val());
        var max = parseInt(thisObject.attr('max'));
        var min = parseInt(thisObject.attr('min'));

        if (value > max) {
            value = max;
            thisObject.val(value);
        }
        if (value < min) {
            value = min;
            thisObject.val(value);
        }

        if (thisObject.val() != '') {
            var field = parseInt(source.css('width')) / (max - min);
            var point = source.find('.slider_point');
            point.css('left', (field * value) - 15);
        }
    });

    $('body').on('mouseup touchend', function (e) {
        if (slider != null)
            slider = null;
    });
});
