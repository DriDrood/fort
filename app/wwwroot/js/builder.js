var Builder = {
    'root': {
        'paths': document.getElementById('paths'),
        'turns': document.getElementById('turns'),
        'cities': document.getElementById('cities')
    },
    'INIT': function (data) {
        // path
        data.paths.forEach(function (path) {
            Builder.path.create(path.id, path.x1, path.y1, path.x2, path.y2, path.color1, path.color2);
        });

        // turns
        data.turns.forEach(function (turn) {
            Builder.turn.createUpdate(turn.sourceCityId, turn.targetCityId, turn.amount, turn.color);
        })

        // city
        data.cities.forEach(function (city) {
            Builder.city.create(city.id, city.x, city.y, city.r, city.color, city.neighbours, city.owned, city.army, city.image);
        });
    },
    'clean': function () {
        document.getElementById('paths').innerHTML = '';
        document.getElementById('turns').innerHTML = '';
        document.getElementById('cities').innerHTML = '';
    },

    'city': {
        'create': function (id, x, y, r, color, neighbours, owned, army, image) {
            var fill = null;
            var style = null;

            // ally
            if (army >= 0) {
                // army size
                Builder._base_.createCircle('cities', x - r, y - r, 12, '#fff', 'stroke:#000;stroke-width:2;', null, { 'for-id': id });
                Builder._base_.createCircleText('cities', x - r, y - r, 12, '#000', army, null, { 'for-id': id });

                fill = image;
                style = 'stroke:' + color + ';stroke-width:2;';
            }

            // enemy
            else {
                fill = color;
            }

            // city circle
            Builder._base_.createCircle('cities', x, y, r, fill, style, 'city-' + id, { 'city-id': id, 'neighbours': neighbours, 'owned': owned, 'army': army });
        },
        'update': function (id, army) {

        },
        'highlight': function (cityId) {
            var city = document.getElementById('city-' + cityId);
            city.setAttribute('filter', 'url(#shadow)');
        },
        'unhighlight': function (cityId) {
            var city = document.getElementById('city-' + cityId);
            city.setAttribute('filter', '');
        }
    },
    'path': {
        'create': function (id, x1, y1, x2, y2, color1, color2) {
            var middleX = Utils.middle(x1, x2);
            var middleY = Utils.middle(y1, y2);

            Builder._base_.createLine('paths', x1, y1, middleX, middleY, 'stroke:' + color1 + ';stroke-width:5', 'path-a-' + id, { 'path-id': id, 'direction': 1 });
            Builder._base_.createLine('paths', x2, y2, middleX, middleY, 'stroke:' + color2 + ';stroke-width:5', 'path-b-' + id, { 'path-id': id, 'dicertion': 2 });
        },
        'get': function (pathId) {
            var first = document.getElementById('path-a-' + pathId);
            var second = document.getElementById('path-b-' + pathId);
            return [first, second];
        },
        'highlightSet': function (pathId) {
            Builder.path.get(pathId).setAttribute('filter', 'url(#shadow)');
        },
        'highlightUnset': function (pathId) {
            Builder.path.get(pathId).removeAttribute('filter');
        }
    },
    'turn': {
        'createUpdate': function (sourceCityId, targetCityId, amount, color) {
            // remove old
            Builder.turn.remove(sourceCityId, targetCityId);
            if (amount <= 0)
                return;

            if (color == null || color === undefined)
                color = document.getElementById('body').getAttribute('data-player-color');

            var pathId = Builder.turn.toPathId(sourceCityId, targetCityId);
            var path = Builder.path.get(pathId)[pathId.startsWith(sourceCityId) ? 0 : 1]; // get path closer to sourceCity
            var middleX = Utils.middle(path.getAttribute('x1'), path.getAttribute('x2'));
            var middleY = Utils.middle(path.getAttribute('y1'), path.getAttribute('y2'));

            Builder.path.highlightSet(pathId);
            Builder._base_.createCircle('turns', middleX, middleY, 12, color, null, 'turn-' + sourceCityId + '-' + targetCityId);
            Builder._base_.createCircleText('turns', middleX, middleY, 12, color, amount, 'turn-text-' + sourceCityId + '-' + targetCityId);
        },
        'remove': function (sourceCityId, targetCityId) {
            var reverse = Builder.turn.get(targetCityId, sourceCityId);
            var turn = Builder.turn.get(sourceCityId, targetCityId);
            if (!turn)
                return;

            // remove turn circle
            turn.forEach(function (item) {
                item.remove();
            });

            // unhighlight
            if (!reverse) {
                Builder.path.highlightUnset(Builder.turn.toPathId(sourceCityId, targetCityId));
            }
        },
        'get': function (sourceCityId, targetCityId) {
            var turn = document.getElementById('turn-' + sourceCityId + '-' + targetCityId);
            if (!turn)
                return false;

            var turnText = document.getElementById('turn-text-' + sourceCityId + '-' + targetCityId);
            if (!turnText)
                return false;

            return [turn, turnText];
        },
        'toPathId': function (sourceCityId, targetCityId) {
            return sourceCityId > targetCityId
                ? targetCityId + '-' + sourceCityId
                : sourceCityId + '-' + targetCityId;
        }
    },
    'animation': function (sourceCityId, targetCityId, radiusStart, radiusEnd, color) {
        
    },

    '_base_': {
        'createCircle': function (rootSection, x, y, r, fill, style, htmlId, otherAttribute) {
            var circle = document.createElementNS("http://www.w3.org/2000/svg", 'circle');
            // mandatory
            circle.setAttribute('cx', x);
            circle.setAttribute('cy', y);
            circle.setAttribute('r', r);
            // optional
            if (fill != null)
                circle.setAttribute('fill', fill);
            if (style != null)
                circle.setAttribute('style', style);
            if (htmlId != null)
                circle.id = htmlId;

            Object.keys(otherAttribute).forEach(function (name) {
                circle.setAttribute('data-' + name, otherAttribute[name]);
            });

            // add
            Builder.root[rootSection].appendChild(circle);
        },
        'createCircleText': function (rootSection, cx, cy, r, color, text, htmlId, otherAttribute) {
            var textObj = document.createElementNS("http://www.w3.org/2000/svg", 'text');
            textObj.setAttribute('x', cx);
            textObj.setAttribute('y', cy + (r / 2));
            textObj.setAttribute('r', r);
            textObj.setAttribute('text-anchor', 'middle');
            Object.keys(otherAttribute).forEach(function (name) {
                textObj.setAttribute('data-' + name, otherAttribute[name]);
            });
            if (htmlId != null)
                textObj.id = htmlId;
            textObj.appendChild(document.createTextNode(text));

            Builder.root[rootSection].appendChild(textObj);
        },
        'createLine': function (rootSection, x1, y1, x2, y2, style, htmlId, otherAttribute) {
            var line = document.createElementNS("http://www.w3.org/2000/svg", 'line');
            line.setAttribute('x1', x1);
            line.setAttribute('y1', y1);
            line.setAttribute('x2', x2);
            line.setAttribute('y2', y2);
            line.setAttribute('style', style);
            Object.keys(otherAttribute).forEach(function (name) {
                line.setAttribute('data-' + name, otherAttribute[name]);
            });
            if (htmlId != null)
                line.id = htmlId;

            Builder.root[rootSection].appendChild(line);
        }
    }
}
