var Builder = {
    'root': {
        'paths': $('#map #paths'),
        'turns': $('#map #turns'),
        'cities': $('#map #cities')
    },
    'INIT': function (data) {
        // path
        $.each(data.paths, function (path) {
            Builder.path.create(path.id, path.x1, path.y1, path.x2, path.y2, path.color1, path.color2);
        });

        // turns
        $.each(data.turns, function (turn) {
            Builder.turn.createUpdate(turn.pathId, turn.reverseDirection, turn.amount);
        })

        // city
        $.each(data.cities, function (city) {
            Builder.city.create(city.id, city.x, city.y, city.color, city.neighbours, city.army, city.image);
        });
    },
    'refresh': function () {
        $('#map_holder').html($('#map_holder').html());
    },
    'city': {
        'create': function (id, x, y, r, color, neighbours, owned, army, image) {
            var fill = null;
            var style = null;

            // ally
            if (army >= 0) {
                // army size
                Builder._base_.createCircle('cities', x - r, y - r, 12, '#fff', 'stroke:#000;stroke-width:2;', null, { 'for-id': id });
                Builder._base_.createCircleText('cities', x - r, y - r, 12, '#000', army, { 'for-id': id });

                fill = image;
                style = 'stroke:' + color + ';stroke-width:2;';
            }

            // enemy
            else {
                fill = color;
            }

            // city circle
            Builder._base_.createCircle('cities', x, y, r, fill, style, 'city-' + id, { 'city-id': id, 'neighbours': neighbours, 'owned': owned });
        },
        'update': function (id, army) {

        },
        'highlight': function (cityId) {
            var city = document.getElementById('city-' + cityId);
            city.setAttribute('filter', 'url(#shadow)');
        }
    },
    'path': {
        'create': function (id, x1, y1, x2, y2, color1, color2) {
            var middleX = utils.middle(x1, x2);
            var middleY = utils.middle(y1, y2);

            Builder._base_.createLine('paths', x1, y1, middleX, middleY, 'stroke:' + color1 + ';stroke-width:5', { 'path-id': id, 'direction': 1 });
            Builder._base_.createLine('paths', x2, y2, middleX, middleY, 'stroke:' + color2 + ';stroke-width:5', { 'path-id': id, 'dicertion': 2 });
        },
        'get': function (pathId) { // TODO: reverseDirection
            return $('[data-path-id=' + pathId + ']');
        },
        'highlightSet': function (pathId) {
            Builder.path.get(pathId).setAttribute('filter', 'url(#shadow)');
        },
        'highlightUnset': function (pathId) {
            Builder.path.get(pathId).removeAttribute('filter');
        }
    },
    'turn': {
        'createUpdate': function (pathId, reverseDirection, amount) {
            // remove old
            Builder.turn.remove(pathId, reverseDirection);
            if (amount <= 0)
                return;

            var path = Builder.path.get(pathId, reverseDirection);
            var middleX = utils.middle(path.attr('x1'), path.attr('x2'));
            var middleY = utils.middle(path.attr('y1'), path.attr('y2'));
            var color = ''; // TODO

            Builder.path.highlightSet(pathId);
            Builder._base_.createCircle(middleX, middleY, 12, color, null, null, { 'turn-path-id': pathId, 'turn-direction': reverseDirection });
            Builder._base_.createCircleText('turns', middleX, middleY, 12, color, amount, { 'turn-path-id': pathId, 'turn-direction': reverseDirection });
        },
        'remove': function (pathId, reverseDirection) {
            Builder.path.highlightUnset(pathId);
            Builder.path.get(pathId, reverseDirection).remove();
        }
    },

    '_base_': {
        'createCircle': function (rootSection, x, y, r, fill, style, htmlId, otherAttribute) {
            var circle = document.createElementNS("http://www.w3.org/2000/svg", 'circle');
            // mandatory
            circle.setAttribute('cx', x);
            circle.setAttribute('cy', y);
            circle.setAttribute('r', r);
            circle.attri
            // optional
            if (fill != null)
                circle.setAttribute('fill', fill);
            if (style != null)
                circle.setAttribute('style', style);
            if (htmlId != null)
                circle.id = htmlId;

            $.each(otherAttribute, function (name, value) {
                circle.setAttribute('data-' + name, value);
            });

            // add
            Builder.root[rootSection].append(circle);
        },
        'createCircleText': function (rootSection, cx, cy, r, color, text, otherAttribute) {
            var text = document.createElementNS("http://www.w3.org/2000/svg", 'text');
            text.setAttribute('x', cx);
            text.setAttribute('y', cy + (r / 2));
            text.setAttribute('r', r);
            text.setAttribute('text-anchor', 'middle');
            $.each(otherAttribute, function (name, value) {
                circle.setAttribute('data-' + name, value);
            });
            text.appendChild(document.createTextNode(text));

            Builder.root[rootSection].append(text);
        },
        'createLine': function (rootSection, x1, y1, x2, y2, style, otherAttribute) {
            var line = document.createElementNS("http://www.w3.org/2000/svg", 'line');
            line.setAttribute('x1', x1);
            line.setAttribute('y1', y1);
            line.setAttribute('x2', x2);
            line.setAttribute('y2', y2);
            line.setAttribute('style', style);
            $.each(otherAttribute, function (name, value) {
                circle.setAttribute('data-' + name, value);
            });

            Builder.root[rootSection].append(line);
        }
    }
}
