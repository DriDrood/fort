import Vue from 'vue';

export default {
  cityRoads: (state) => {
    var cityRoads = {};
    state.roads.forEach(road => {
      const [start, end] = road.split('__');

      if (cityRoads[start] === undefined)
        Vue.set(cityRoads, start, []);
      cityRoads[start].push(end);
      
      if (cityRoads[end] === undefined)
        Vue.set(cityRoads, end, []);
      cityRoads[end].push(start);
    });

    return cityRoads;
  }
}