import Vue from "vue";

export default {
  state: () => ({
    cities: {
      // '1': {
      //   id: '1',
      //   x: 500,
      //   y: 500
      // },
    },
    roads: [
      // '1__2',
      // '1__3',
      // '3__4'
    ]
  }),
  getters: {
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
  },
  mutations: {
    // cities, roads
    mapInit(state, payload) {
      state.cities = payload.cities;
      state.roads = payload.roads;
    },
  },
  actions: {
  },
}
